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
    public int Age
    {
        get => CalculateAge(DateOfBirth);
    }
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
    /// Gets or sets whether this user account needs manual verification
    /// True = pending verification, False = verified/active
    /// </summary>
    /// <value>
    ///   <c>true</c> if [needs verification]; otherwise, <c>false</c>.
    /// </value>
    public bool NeedsVerification { get; private set; }
    /// <summary>
    /// Gets or sets whether this user is a patient
    /// True = patient user, False = employee user
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is patient; otherwise, <c>false</c>.
    /// </value>
    public bool IsPatient { get; private set; }
    /// <summary>
    /// Parameterless constructor required by EF Core
    /// </summary>
    public User() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="fullName">The full name.</param>
    /// <param name="phoneNumber">The phone number.</param>
    /// <param name="email">The email.</param>
    /// <param name="hashedPassword">The hashed password.</param>
    /// <param name="gender">if set to <c>true</c> [gender].</param>
    /// <param name="identityNumber">The identity number.</param>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <param name="address">The address.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="isPatient">if set to <c>true</c> [is patient].</param>
    public User(string fullName, string phoneNumber, string email, string hashedPassword, bool gender, string identityNumber, DateOnly dateOfBirth, string address, int roleId , bool isPatient = false)
    {
        //Validate required fields
        ValidateFullName(fullName);
        ValidatePhoneNumber(phoneNumber);
        ValidateEmail(email);
        ValidateHashedPassword(hashedPassword);
        ValidateIdentityNumber(identityNumber);
        ValidateAddress(address);

        //Generate new GUID for userID
        UserId = Guid.NewGuid();
        // Assign validated values
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Email = email.ToLower();
        HashedPassword = hashedPassword;
        Gender = gender;
        IdentityNumber = identityNumber;
        DateOfBirth = dateOfBirth;
        Address = address;
        RoleId = roleId;
        IsPatient = isPatient;
        // All new users need verification by default
        NeedsVerification = true;
    }
    
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
    
    /// <summary>
    /// Validates the full name.
    /// </summary>
    /// <param name="fullName">The full name.</param>
    /// <exception cref="System.ArgumentException">
    /// Full name cannot be null or empty - fullName
    /// or
    /// Full name must be at least 2 characters - fullName
    /// or
    /// Full name cannot exceed 100 characters - fullName
    /// </exception>
    private static void ValidateFullName(string fullName)
    {
        if(string.IsNullOrEmpty(fullName))
            throw new ArgumentException("Full name cannot be null or empty", nameof(fullName));
        if (fullName.Length < 2)
            throw new ArgumentException("Full name must be at least 2 characters", nameof(fullName));
        if (fullName.Length > 100)
            throw new ArgumentException("Full name cannot exceed 100 characters", nameof(fullName));
    }
    /// <summary>
    /// Validates the phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number.</param>
    /// <exception cref="System.ArgumentException">
    /// Phone number cannot be null or empty - phoneNumber
    /// or
    /// Phone number must contain only digits, start with 0, and be exactly 10 digits long. - phoneNumber
    /// </exception>
    private static void ValidatePhoneNumber(string phoneNumber)
    {
        if(string.IsNullOrEmpty(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));
        //Clean phone number format
        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        if (!cleaned.All(char.IsDigit) || !cleaned.StartsWith('0') || cleaned.Length != 10)
            throw new ArgumentException("Phone number must contain only digits, start with 0, and be exactly 10 digits long.", nameof(phoneNumber));
    }
    /// <summary>
    /// Validates the email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <exception cref="System.ArgumentException">
    /// Email cannot be null or empty - email
    /// or
    /// Email must be in a valid format - email
    /// </exception>
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        if (!email.Contains('@') || !email.Contains('.'))
            throw new ArgumentException("Email must be in a valid format", nameof(email));
        var parts = email.Split('@');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new ArgumentException("Email must be in a valid format", nameof(email));
    }
    /// <summary>
    /// Validates the hashed password.
    /// </summary>
    /// <param name="hashedPassword">The hashed password.</param>
    /// <exception cref="System.ArgumentException">Hashed password cannot be null or empty - hashedPassword</exception>
    private static void ValidateHashedPassword(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Hashed password cannot be null or empty", nameof(hashedPassword));
    }
    /// <summary>
    /// Validates the identity number.
    /// </summary>
    /// <param name="identityNumber">The identity number.</param>
    /// <exception cref="System.ArgumentException">
    /// Identity number cannot be null or empty. - identityNumber
    /// or
    /// Identity number must contain only digits and be exactly 12 digits long. - identityNumber
    /// </exception>
    private static void ValidateIdentityNumber(string identityNumber)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            throw new ArgumentException("Identity number cannot be null or empty.", nameof(identityNumber));

        // Check that it contains only digits and has exactly 12 characters
        if (!identityNumber.All(char.IsDigit) || identityNumber.Length != 12)
            throw new ArgumentException("Identity number must contain only digits and be exactly 12 digits long.", nameof(identityNumber));
    }
    /// <summary>
    /// Validates the address.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <exception cref="System.ArgumentException">Address cannot be null or empty - address</exception>
    private static void ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty", nameof(address));
    }


}
