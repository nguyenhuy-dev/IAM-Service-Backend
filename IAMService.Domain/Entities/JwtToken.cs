namespace IAMService.Domain.Entities
{
    /// <summary>
    ///     Jwt Token entity.
    /// </summary>
    public class JwtToken
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the access token.
        /// </summary>
        /// <value>
        ///     The access token.
        /// </value>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the refresh token.
        /// </summary>
        /// <value>
        ///     The refresh token.
        /// </value>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the re token expire at.
        /// </summary>
        /// <value>
        ///     The re token expire at.
        /// </value>
        public DateTime ReTokenExpireAt { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is revoked.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is revoked; otherwise, <c>false</c>.
        /// </value>
        public bool IsRevoked { get; set; }

        /// <summary>
        ///     Gets or sets the create at.
        /// </summary>
        /// <value>
        ///     The create at.
        /// </value>
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///     Gets or sets the user identifier.
        /// </summary>
        /// <value>
        ///     The user identifier.
        /// </value>
        public Guid UserId { get; set; }

        /// <summary>
        ///     Gets or sets the user.
        /// </summary>
        /// <value>
        ///     The user.
        /// </value>
        public User User { get; set; } = default!;
    }
}
