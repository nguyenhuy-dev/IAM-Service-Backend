using IAMService.Application.DTOs.Auth.Login;
namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for the core authentication and authorization services.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with the provided credentials and generates a token pair.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>A <see cref="TokenResponse"/> containing the access and refresh tokens, and user details.</returns>
        Task<TokenResponse> LoginAsync(string email, string password);

        /// <summary>
        /// Invalidates and revokes a refresh token, effectively logging the user out across all sessions using that token.
        /// </summary>
        /// <param name="refreshToken">The refresh token string to be revoked.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogoutAsync(string refreshToken);

        /// <summary>
        /// Checks the validity and active status of a given token (usually a refresh token).
        /// </summary>
        /// <param name="token">The token string to check.</param>
        /// <returns>A task that returns true if the token is active, valid, and not revoked; otherwise, false.</returns>
        Task<bool> IsActive(string token);

        /// <summary>
        /// Initiates the forgot password process for a user, typically by sending a recovery link or code.
        /// </summary>
        /// <param name="username">The username or email address associated with the account.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ForgotPassword(string username);
    }
}