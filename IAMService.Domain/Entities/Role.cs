namespace IAMService.Domain.Entities;

/// <summary>
/// Role entity representing a user role in the system.
/// </summary>
public class Role
{
    /// <summary>
    /// Gets or sets the role identifier.
    /// </summary>
    /// <value>
    /// The role identifier.
    /// </value>
    public int RoleId { get; set; }
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    /// <value>
    /// The name of the role.
    /// </value>
    public string RoleName { get; set; } = default!;
    /// <summary>
    /// Gets or sets the role code.
    /// </summary>
    /// <value>
    /// The role code.
    /// </value>
    public string RoleCode { get; set; } = default!;
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description { get; set; } = default!;
    /// <summary>
    /// Gets or sets the users.
    /// </summary>
    /// <value>
    /// The users.
    /// </value>
    public ICollection<User> Users { get; set; } = [];
    /// <summary>
    /// Gets or sets the privileges.
    /// </summary>
    /// <value>
    /// The privileges.
    /// </value>
    public ICollection<Privilege> Privileges { get; set; } = [];
}
