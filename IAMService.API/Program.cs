using FluentValidation;
using IAMService.API.Middleware;
using IAMService.Application;
using IAMService.Application.Behaviors;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AccessToken;
using IAMService.Application.Interfaces.ForgetPassword;
using IAMService.Application.Mappings;
using IAMService.Application.Services;
using IAMService.Infrastructure.Repositories;
using IAMService.Infrastructure.Repositories.ForgetPassword;
using IAMService.Infrastructure.Services;
using IAMService.Infrastructure.Services.AccessToken;
using IAMService.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var redisConnectionString = configuration.GetConnectionString("RedisConnection");

builder.Services.AddDbContext<IAMServiceDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString ?? throw new InvalidOperationException("Redis connection string is missing."))
);
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var signingKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is required.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

        
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        
        ValidateLifetime = true,
        
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var accessToken = context.Request.Headers["Authorization"]
            .FirstOrDefault()?
            .Replace("Bearer ", string.Empty);
            var serviceProvider = context.HttpContext.RequestServices;

            var tokenDecoderService = serviceProvider.GetRequiredService<ITokenDecoderService>();
            var invalidationService = serviceProvider.GetRequiredService<IInvalidationService>();

            string tokenIdentifier = string.Empty;
            try
            {
                tokenIdentifier = tokenDecoderService.GetTokenIdentifier(accessToken);
            }
            catch (Exception)
            {
                context.Fail("Invalid Token structure: JTI missing or unreadable.");
            }
            if (string.IsNullOrEmpty(tokenIdentifier))
            {
                context.Fail("JTI claim is missing");
                return;
            }
            bool isRevoked = await invalidationService.IsTokenInvalidatedAsync(tokenIdentifier);
            if (isRevoked)
            {
                context.Fail("The access token has been explicitly revoked (user logged out).");
                return;
            }
        }
    };
})
.AddLabToken(configureOptions =>
{
    configureOptions.IssuerSigningKey = configuration.GetSection("Jwt")["Secret"] ?? "";
    configureOptions.ValidIssuer = configuration.GetSection("Jwt")["Issuer"] ?? "";
    configureOptions.ValidAudience = configuration.GetSection("Jwt")["Audience"] ?? "";
});

builder.Services.AddAuthorization();
builder.Services.AddLabAuthorization();
builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization", LogLevel.Debug);

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepositoy>();

builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IRoleCloneService, RoleCloneService>();
builder.Services.AddScoped<IJwtConfiguration, JwtConfiguration>();
builder.Services.AddTransient<ITokenGenerator, GenerateTokenService>();
builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITokenHasher, TokenHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IInvalidationService, InvalidationAccessTokenService>();
builder.Services.AddTransient<ITokenDecoderService, TokenDecoderService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IAssemblyReference).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(IAssemblyReference).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfile));


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference().AllowAnonymous();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok("Welcome to IAM Service")).AllowAnonymous();

await app.MigrateDbContextAsync<IAMServiceDbContext>();

app.Run();
