namespace IAMService.Domain.Entities;

/// <summary>
/// User Token entity class.
/// </summary>
public class UserToken
{
    /// <summary>
    /// Gets or sets the user token identifier.
    /// </summary>
    /// <value>
    /// The user token identifier.
    /// </value>
    public Guid UserTokenId { get; set; }
    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    /// <value>
    /// The token.
    /// </value>
    public string Token { get; set; } = default!;
    /// <summary>
    /// Gets or sets the expiration at.
    /// </summary>
    /// <value>
    /// The expiration at.
    /// </value>
    public DateTime ExpirationAt { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this instance is revoked.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is revoked; otherwise, <c>false</c>.
    /// </value>
    public bool IsRevoked { get; set; }
    /// <summary>
    /// Gets or sets the create at.
    /// </summary>
    /// <value>
    /// The create at.
    /// </value>
    public DateTime CreateAt { get; set; }
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    /// <value>
    /// The user.
    /// </value>
    public User User { get; set; } = default!;
}
