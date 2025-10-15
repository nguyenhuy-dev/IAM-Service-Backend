namespace IAMService.Domain.Entities;

/// <summary>
/// Privilege entity class.
/// </summary>
public class Privilege
{
    /// <summary>
    /// Gets or sets the privilege identifier.
    /// </summary>
    /// <value>
    /// The privilege identifier.
    /// </value>
    public int PrivilegeId { get; set; }
    /// <summary>
    /// Gets or sets the name of the privilege.
    /// </summary>
    /// <value>
    /// The name of the privilege.
    /// </value>
    public string PrivilegeName { get; set; } = default!;
    /// <summary>
    /// Gets or sets the roles.
    /// </summary>
    /// <value>
    /// The roles.
    /// </value>
    public ICollection<Role> Roles { get; set; } = [];
}
