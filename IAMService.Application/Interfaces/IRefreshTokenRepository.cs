using IAMService.Domain.Entities;
namespace IAMService.Application.Interfaces
{
    /// <summary>
    ///     Defines the contract for persistence operations related to the <see cref="RefreshToken" /> entity.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        ///     Adds a new refresh token to the repository.
        /// </summary>
        /// <param name="refreshToken">The refresh token entity to add.</param>
        /// <returns>A task that returns the added <see cref="RefreshToken" /> entity, potentially with updated ID/timestamps.</returns>
        Task<RefreshToken> AddAsync(RefreshToken refreshToken);

        /// <summary>
        ///     Retrieves a refresh token by its unique identifier (ID).
        /// </summary>
        /// <param name="tokenId">The unique identifier of the token.</param>
        /// <returns>A task that returns the matching <see cref="RefreshToken" /> entity, or null if not found.</returns>
        Task<RefreshToken?> GetByIdAsync(Guid tokenId);

        /// <summary>
        ///     Retrieves a refresh token by its hashed token value.
        /// </summary>
        /// <param name="tokenHash">The hashed value of the token.</param>
        /// <returns>A task that returns the matching <see cref="RefreshToken" /> entity, or null if not found.</returns>
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

        /// <summary>
        ///     Retrieves all active (not expired and not revoked) refresh tokens belonging to a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task that returns a list of active <see cref="RefreshToken" /> entities.</returns>
        Task<List<RefreshToken>> GetActiveTokenByUserIdAsync(Guid userId);

        /// <summary>
        ///     Updates the state of an existing refresh token entity (e.g., revoking it).
        /// </summary>
        /// <param name="refreshToken">The refresh token entity with updated properties.</param>
        /// <returns>A task that returns true if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateAsync(RefreshToken refreshToken);

        /// <summary>
        ///     Removes all expired and/or revoked tokens from the repository to maintain cleanliness.
        /// </summary>
        /// <returns>A task that returns the number of tokens deleted during the cleanup operation.</returns>
        Task<int> CleanupExpiredTokenAsync();
    }
}
