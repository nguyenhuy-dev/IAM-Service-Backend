using FluentValidation;
using IAMService.API.Middleware;
using IAMService.Application;
using IAMService.Application.Behaviors;
using IAMService.Application.Interfaces;
using IAMService.Application.Mappings;
using IAMService.Infrastructure.Repositories;
using IAMService.Infrastructure.Services;
using IAMService.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    
    var signingKey = builder.Configuration["Jwt:SigningKey"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!)),

        
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"]!,

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]!,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
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
builder.Services.AddScoped<IRoleCloneService, RoleCloneService>();

builder.Services.AddScoped<IJwtConfiguration, JwtConfiguration>();
builder.Services.AddTransient<ITokenGenerator, GenerateTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddTransient<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITokenHasher, TokenHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
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
