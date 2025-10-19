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
    public int RoleId { get; private set; }
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    /// <value>
    /// The name of the role.
    /// </value>
    public string RoleName { get; private set; } = default!;
    /// <summary>
    /// Gets or sets the role code.
    /// </summary>
    /// <value>
    /// The role code.
    /// </value>
    public string RoleCode { get; private set; } = default!;
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string Description { get; private set; } = default!;
    /// <summary>
    /// Check if the role is default or not.
    /// </summary>
    /// <value>
    /// The isDefault.
    /// </value>
    public bool IsDefault { get; set; } = false;
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

    public Role() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Role"/> class
    /// </summary>
    /// <param name="roleId">The role id</param>
    /// <param name="roleName">The role name</param>
    /// <param name="roleCode">The role code</param>
    /// <param name="description">The description</param>
    /// <exception cref="ArgumentException">Description cannot be null or empty </exception>
    /// <exception cref="ArgumentException">Role code cannot be null or empty </exception>
    /// <exception cref="ArgumentException">Role name cannot be null or empty </exception>
    public Role(int roleId, string roleName, string roleCode, string description)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code cannot be null or empty", nameof(roleCode));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        RoleId = roleId;
        RoleName = roleName;
        RoleCode = roleCode;
        Description = description;
    }

    /// <summary>
    /// Updates the role name using the specified new role name
    /// </summary>
    /// <param name="newRoleName">The new role name</param>
    /// <exception cref="ArgumentException">Role name cannot be null or empty </exception>
    public void UpdateRoleName(string newRoleName)
    {
        if (string.IsNullOrWhiteSpace(newRoleName))
            throw new ArgumentException("Role name cannot be null or empty", nameof(newRoleName));
        RoleName = newRoleName;
    }

    /// <summary>
    /// Updates the role code using the specified new role code
    /// </summary>
    /// <param name="newRoleCode">The new role code</param>
    /// <exception cref="ArgumentException">Role code cannot be null or empty </exception>
    public void UpdateRoleCode(string newRoleCode)
    {
        if (string.IsNullOrWhiteSpace(newRoleCode))
            throw new ArgumentException("Role code cannot be null or empty", nameof(newRoleCode));
        RoleCode = newRoleCode;
    }

    /// <summary>
    /// Updates the description using the specified new description
    /// </summary>
    /// <param name="newDescription">The new description</param>
    /// <exception cref="ArgumentException">Description cannot be null or empty </exception>
    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Description cannot be null or empty", nameof(newDescription));
        Description = newDescription;
    }
}
