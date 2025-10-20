using System.ComponentModel.DataAnnotations.Schema;

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
    
    [NotMapped]
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

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User() { }

    /// <summary>
    /// Updates the user's information.
    /// <para>
    /// This method allows for partial updates. Fields that are not provided remain unchanged.
    /// </para>
    /// </summary>
    /// <param name="fullName">The new full name (optional).</param>
    /// <param name="phoneNumber">The new phone number (optional).</param>
    /// <param name="email">The new email address (optional).</param>
    /// <param name="gender">The new gender value (optional).</param>
    /// <param name="identityNumber">The new identity number (optional).</param>
    /// <param name="dateOfBirth">The new date of birth (optional).</param>
    /// <param name="address">The new address (optional).</param>
    public void UpdateUser(
        string? fullName = null,
        string? phoneNumber = null,
        string? email = null,
        bool? gender = null,
        string? identityNumber = null,
        DateOnly? dateOfBirth = null,
        string? address = null)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            FullName = fullName;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            PhoneNumber = phoneNumber;

        if (!string.IsNullOrWhiteSpace(email))
            Email = email;

        if (gender.HasValue)
            Gender = gender.Value;

        if (!string.IsNullOrWhiteSpace(identityNumber))
            IdentityNumber = identityNumber;

        if (dateOfBirth.HasValue)
        {
            DateOfBirth = dateOfBirth.Value;
            Age = CalculateAge(dateOfBirth.Value);
        }

        if (!string.IsNullOrWhiteSpace(address))
            Address = address;
    }

    /// <summary>
    /// Calculate age automatically based on date of birth.
    /// </summary>
    private static int CalculateAge(DateOnly dob)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }


}
