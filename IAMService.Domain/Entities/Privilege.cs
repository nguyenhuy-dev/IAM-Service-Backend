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

    public Privilege() { }

    public ICollection<Role> Roles { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Privilege"/> class
    /// </summary>
    /// <param name="privilegeName">The privilege name</param>
    /// <exception cref="ArgumentException">Privilege Name cannot be null or empty </exception>
    
    public Privilege()
    {
        // Parameterless constructor for EF Core
    }
    public Privilege(string privilegeName)
    {
        if (string.IsNullOrWhiteSpace(privilegeName))
            throw new ArgumentException("Privilege Name cannot be null or empty", nameof(privilegeName));

        PrivilegeName = privilegeName;
    }

    /// <summary>
    /// Updates the privilege name using the specified new privilege name
    /// </summary>
    /// <param name="newPrivilegeName">The new privilege name</param>
    /// <exception cref="ArgumentException">Privilege Name cannot be null or empty </exception>
    public void UpdatePrivilegeName(string newPrivilegeName)
    {
        if (string.IsNullOrWhiteSpace(newPrivilegeName))
            throw new ArgumentException("Privilege Name cannot be null or empty", nameof(newPrivilegeName));

        PrivilegeName = newPrivilegeName;
    }
}
