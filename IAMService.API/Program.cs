using FluentValidation;
using IAMService.API.Middleware;
using IAMService.Application;
using IAMService.Application.Behaviors;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AccessToken;
using IAMService.Application.Mappings;
using IAMService.Application.Services;
using IAMService.Infrastructure.Repositories;
using IAMService.Infrastructure.Services;
using IAMService.Infrastructure.Services.AccessToken;
using IAMService.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
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
});


builder.Services.AddControllers();

builder.Services.AddOpenApi();

var configuration = builder.Configuration;
builder.Services.AddDbContext<IAMServiceDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.Configure<EmailSettings>(
builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase()
);
builder.Services.AddScoped<IRoleCloneService, RoleCloneService>();
builder.Services.AddScoped<IJwtConfiguration, JwtConfiguration>();
builder.Services.AddTransient<ITokenGenerator, GenerateTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITokenHasher, TokenHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IInvalidationService, InvalidationAccessTokenService>();
builder.Services.AddTransient<ITokenDecoderService, TokenDecoderService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString)
);
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IAssemblyReference).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddProblemDetails(); 
builder.Services.AddValidatorsFromAssembly(typeof(IAssemblyReference).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfile));


builder.Services.AddAuthorization(); 

builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization", LogLevel.Debug);

builder.Services.AddAuthentication("Token")
    .AddLabToken(configureOptions =>
        {
            configureOptions.IssuerSigningKey = configuration.GetSection("Jwt")["Secret"] ?? "";
            configureOptions.ValidIssuer = configuration.GetSection("Jwt")["Issuer"] ?? "";
            configureOptions.ValidAudience = configuration.GetSection("Jwt")["Audience"] ?? "";
        }
    );
builder.Services.AddLabAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapGet("/", () => Results.Ok("Welcome to IAM Service")).AllowAnonymous();

app.MapScalarApiReference().AllowAnonymous();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.MigrateDbContextAsync<IAMServiceDbContext>();

app.Run();
