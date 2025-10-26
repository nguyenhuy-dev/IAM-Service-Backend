namespace IAMService.Application.DTOs.AuthDTOs;

/// <summary>
/// Refresh token user data tranfer object.
/// </summary>
public class RefreshTokenUserDto
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    /// <value>
    /// The email.
    /// </value>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    /// <value>
    /// The full name.
    /// </value>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role code.
    /// </summary>
    /// <value>
    /// The role code.
    /// </value>
    public string RoleCode { get; set; } = string.Empty;
}
