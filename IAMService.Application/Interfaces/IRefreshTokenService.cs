using IAMService.Application.DTOs.Auth.Login;

namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for business logic related to creating, managing, and revoking refresh tokens.
    /// This service handles token rotation and persistence operations.
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Generates a new access token and a new refresh token for a user, persists the refresh token, and saves changes.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The collection of roles assigned to the user, used for claim generation.</param>
        /// <returns>A task that returns a <see cref="TokenResponse"/> containing the new access and refresh tokens.</returns>
        Task<TokenResponse> CreateTokensAndSaveChanges(Guid userId, string roleCode);

        /// <summary>
        /// Rotates a refresh token by revoking the old token and issuing a new token pair.
        /// </summary>
        /// <param name="oldRefreshTokenString">The string value of the refresh token to be revoked.</param>
        /// <returns>A task that returns a <see cref="TokenResponse"/> containing the new access and refresh tokens.</returns>
        Task<TokenResponse> RotateAndRevokeAsync(string oldRefreshTokenString);

        /// <summary>
        /// Revokes all active refresh tokens associated with a specific user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose tokens should be revoked.</param>
        /// <returns>A task that returns true if tokens were successfully revoked; otherwise, false.</returns>
        Task<bool> RevokeAllTokensForUserAsync(Guid userId);

        /// <summary>
        /// Revokes a specific refresh token using its unique ID.
        /// </summary>
        /// <param name="tokenId">The unique identifier of the token to revoke.</param>
        /// <returns>A task that returns true if the token was successfully revoked; otherwise, false.</returns>
        Task<bool> RevokeTokenByIdAsync(Guid tokenId);

        /// <summary>
        /// Revokes a specific refresh token using its string value.
        /// </summary>
        /// <param name="refreshTokenString">The string value of the refresh token to revoke.</param>
        /// <returns>A task that returns true if the token was successfully revoked; otherwise, false.</returns>
        Task<bool> RevokeTokenByStringAsync(string refreshTokenString);

        /// <summary>
        /// Performs cleanup by deleting all expired and revoked refresh tokens from the persistence store.
        /// </summary>
        /// <returns>A task that returns the count of tokens that were removed.</returns>
        Task<int> CleanupExpiredTokenAsync();
    }
}