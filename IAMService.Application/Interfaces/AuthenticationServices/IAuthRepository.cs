using IAMService.Domain.Entities;

namespace IAMService.Application.Interfaces.AuthenticationServices;

/// <summary>
/// Interface for authentication repository.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Checks the valid token.
    /// </summary>
    /// <param name="tokenValue">The token value.</param>
    /// <returns></returns>
    Task<bool> CheckValidToken(string tokenValue);

    /// <summary>
    /// Adds the JWT token.
    /// </summary>
    /// <param name="jwtToken">The JWT token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task AddJwtToken(JwtToken jwtToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logins the specified email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<User> Login(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all JWT tokens.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<int> DeleteAllJwtTokens(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user with old access token.
    /// </summary>
    /// <param name="oldAccessToken">The old access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<User> GetUserWithOldAccessToken(string oldAccessToken, CancellationToken cancellationToken = default);
}
