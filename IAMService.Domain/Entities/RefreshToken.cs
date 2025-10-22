using System;

namespace IAMService.Domain.Entities
{
    /// <summary>
    /// Represents a refresh token entity used for non-interactive token renewal.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets the unique identifier for the refresh token.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who owns this token.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the user who owns this token.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets the hashed value of the refresh token.
        /// </summary>
        public string TokenHash { get; private set; }

        /// <summary>
        /// Gets the date and time (UTC) when the refresh token expires.
        /// </summary>
        public DateTime ExpiresAt { get; private set; }

        /// <summary>
        /// Gets the date and time (UTC) when the refresh token was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time (UTC) when the token was revoked, if applicable.
        /// </summary>
        public DateTime? RevokedAt { get; private set; }

        /// <summary>
        /// Gets or sets the ID of the new token that replaced this one, if revoked as part of a rotation.
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the token that replaced this one.
        /// </summary>
        public RefreshToken ReplacedByToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshToken"/> class (required for ORMs).
        /// </summary>
        public RefreshToken() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshToken"/> class with creation details.
        /// </summary>
        /// <param name="userId">The ID of the user the token belongs to.</param>
        /// <param name="tokenHash">The unique hashed value of the token.</param>
        /// <param name="expiresAt">The expiration date and time (UTC).</param>
        public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
        {
            UserId = userId;
            TokenHash = tokenHash;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Revokes the refresh token, setting the revocation date and optionally linking it to the replacement token.
        /// </summary>
        /// <param name="replacedByTokenId">The ID of the new token that replaced this one during rotation.</param>
        public void Revoke(Guid? replacedByTokenId = null)
        {
            if (!RevokedAt.HasValue)
            {
                RevokedAt = DateTime.UtcNow;
                ReplacedByTokenId = replacedByTokenId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the token is currently valid (not revoked and not expired).
        /// </summary>
        public bool IsActive => !RevokedAt.HasValue && ExpiresAt > DateTime.UtcNow;
    }
}