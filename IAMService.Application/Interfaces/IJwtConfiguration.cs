
namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Defines the necessary JWT configuration settings required by the application's business logic.
    /// This abstraction decouples the Application layer from the Infrastructure's IConfiguration binder.
    /// </summary>
    public interface IJwtConfiguration
    {
        /// <summary>
        /// Gets the lifetime of the refresh token in days.
        /// </summary>
        int RefreshTokenLifetimeDays { get; }

        /// <summary>
        /// Gets the lifetime of the access token in seconds.
        /// (This is primarily used by the Infrastructure layer, but can be defined here for completeness).
        /// </summary>
        int AccessTokenLifetimeSeconds { get; }
        // Có thể thêm SigningKey, Issuer, Audience nếu cần thiết, nhưng thường để ở Infrastructure.

        /// <summary>
        /// Gets the maximum number of failed access attempts before a user is locked out.
        /// </summary>
        int MaxFailedAccessAttempts { get; }

        /// <summary>
        /// Gets the duration (in minutes) an account is locked out for.
        /// </summary>
        int DefaultLockoutMinutes { get; }
    }
}
