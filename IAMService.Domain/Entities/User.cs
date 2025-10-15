namespace IAMService.Domain.Entities;

/// <summary>
/// User entity class.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    /// <value>
    /// The full name.
    /// </value>
    public string FullName { get; set; } = default!;
    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    /// <value>
    /// The phone number.
    /// </value>
    public string PhoneNumber { get; set; } = default!;
    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    /// <value>
    /// The email.
    /// </value>
    public string Email { get; set; } = default!;
    /// <summary>
    /// Gets or sets the hashed password.
    /// </summary>
    /// <value>
    /// The hashed password.
    /// </value>
    public string HashedPassword { get; set; } = default!;
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="User"/> is gender.
    /// </summary>
    /// <value>
    ///   <c>true</c> if gender; otherwise, <c>false</c>.
    /// </value>
    public bool Gender { get; set; }
    /// <summary>
    /// Gets or sets the identity number.
    /// </summary>
    /// <value>
    /// The identity number.
    /// </value>
    public string IdentityNumber { get; set; } = default!;
    /// <summary>
    /// Gets or sets the age.
    /// </summary>
    /// <value>
    /// The age.
    /// </value>
    public int Age { get; set; }
    /// <summary>
    /// Gets or sets the date of birth.
    /// </summary>
    /// <value>
    /// The date of birth.
    /// </value>
    public DateOnly DateOfBirth { get; set; }
    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    /// <value>
    /// The address.
    /// </value>
    public string Address { get; set; } = default!;
    /// <summary>
    /// Gets or sets the role identifier.
    /// </summary>
    /// <value>
    /// The role identifier.
    /// </value>
    public int RoleId { get; set; }
    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    /// <value>
    /// The role.
    /// </value>
    public Role Role { get; set; } = default!;
}
