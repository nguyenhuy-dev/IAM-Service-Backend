using IAMService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
namespace IAMService.Infrastructure.Services
{
    /// <summary>
    ///     Implementation of IJwtConfiguration that binds configuration values from the application's
    ///     configuration source (e.g., appsettings.json) with default fallback values.
    /// </summary>
    public class JwtConfiguration : IJwtConfiguration
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="JwtConfiguration" /> class by binding configuration values.
        /// </summary>
        /// <param name="configuration">The application configuration source.</param>
        public JwtConfiguration(IConfiguration configuration)
        {
            // Token Lifetime Settings
            RefreshTokenLifetimeDays = configuration.GetValue("Jwt:RefreshTokenLifetimeDays", 14);
            AccessTokenLifetimeSeconds = configuration.GetValue("Jwt:AccessTokenLifetimeSeconds", 900); // 15 minutes

            // Lockout Policy Settings
            MaxFailedAccessAttempts = configuration.GetValue("Lockout:MaxFailedAccessAttempts", 5);
            DefaultLockoutMinutes = configuration.GetValue("Lockout:DefaultLockoutMinutes", 15);
        }
        /// <summary>
        ///     Gets the duration, in days, until a refresh token expires.
        /// </summary>
        public int RefreshTokenLifetimeDays { get; }

        /// <summary>
        ///     Gets the duration, in seconds, until an access token expires.
        /// </summary>
        public int AccessTokenLifetimeSeconds { get; }

        /// <summary>
        ///     Gets the maximum number of failed login attempts allowed before a user account is locked out.
        /// </summary>
        public int MaxFailedAccessAttempts { get; }

        /// <summary>
        ///     Gets the duration, in minutes, that a user account remains locked out after exceeding the maximum failed access
        ///     attempts.
        /// </summary>
        public int DefaultLockoutMinutes { get; }
    }
}
