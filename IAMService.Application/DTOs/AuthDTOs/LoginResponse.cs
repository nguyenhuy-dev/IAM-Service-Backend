namespace IAMService.Application.DTOs.AuthDTOs;

/// <summary>
/// Login response model.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the full name.
    /// </summary>
    /// <value>
    /// The full name.
    /// </value>
    public string FullName { get; init; } = default!;

    /// <summary>
    /// Gets the email.
    /// </summary>
    /// <value>
    /// The email.
    /// </value>
    public string Email { get; init; } = default!;

    /// <summary>
    /// Gets or sets the role code.
    /// </summary>
    /// <value>
    /// The role code.
    /// </value>
    public string RoleCode { get; set; } = default!;

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    /// <value>
    /// The access token.
    /// </value>
    public string AccessToken { get; set; } = default!;
}
