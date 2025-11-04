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
using Scalar.AspNetCore;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/run/secrets/secrets_file", optional: true);

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
builder.Services.AddTransient<IInvalidationService, InvalidationAccessTokenService>();
builder.Services.AddTransient<ITokenDecoderService, TokenDecoderService>();
var encryptionPassphrase = configuration["ENCRYPTION_PASSPHRASE"] ??
    Environment.GetEnvironmentVariable("ENCRYPTION_PASSPHRASE");

if (string.IsNullOrEmpty(encryptionPassphrase))
{
    throw new InvalidOperationException(
        "ENCRYPTION_PASSPHRASE must be set in User Secrets (Development) or Environment Variables (Production)");
}

// Register encryption service as singleton
builder.Services.AddSingleton<IStringEncryptionService>(sp =>
    new IAMService.Infrastructure.Services.StringEncryptionService(encryptionPassphrase));
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IAssemblyReference).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(IAssemblyReference).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization", LogLevel.Debug);
builder.Services.AddAuthentication("Token")
    .AddLabToken(configureOptions =>
        {
            var sectionJwt = configuration.GetSection("Jwt");
            configureOptions.IssuerSigningKey = sectionJwt["SigningKey"] ?? "";
            configureOptions.ValidIssuer = sectionJwt["Issuer"] ?? "";
            configureOptions.ValidAudience = sectionJwt["Audience"] ?? "";
        }
    );
builder.Services.AddLabAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddCorsLab("AllowExternal", ["http://localhost:5173"]);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173") // FE chạy ở đây
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowExternal");

app.MapScalarApiReference().AllowAnonymous();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok("Welcome to IAM Service")).AllowAnonymous();

await app.MigrateDbContextAsync<IAMServiceDbContext>();

app.Run();
