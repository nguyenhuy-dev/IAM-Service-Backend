namespace IAMService.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for User entity
    /// Used to return user information to API clients
    /// Does NOT include sensitive information like HashedPassword
    /// </summary>
    public record UserDto
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public Guid UserId { get; init; }
        /// <summary>
        /// Gets or sets the user's full name
        /// </summary>
        public required string FullName { get; init; }
        /// <summary>
        /// Gets or sets the user's phone number
        /// </summary>
        public required string PhoneNumber { get; init; }
        /// <summary>
        /// Gets or sets the user's email address
        /// </summary>
        public required string Email { get; init; }
        /// <summary>
        /// Gets or sets the user's gender
        /// "Male" or "Female"
        /// </summary>
        public required string Gender { get; init; }
        /// <summary>
        /// Gets or sets the user's identity number
        /// </summary>
        public required string IdentityNumber { get; init; }
        /// <summary>
        /// Gets or sets the user's age
        /// </summary>
        public int Age { get; init; }
        /// <summary>
        /// Gets or sets the user's date of birth
        /// </summary>
        public DateOnly DateOfBirth { get; init; }
        /// <summary>
        /// Gets or sets the user's address
        /// </summary>
        public required string Address { get; init; }
        /// <summary>
        /// Gets or sets the role information
        /// </summary>
        public required RoleDto Role { get; init; }
        /// <summary>
        /// Gets or sets whether the account needs verification
        /// </summary>
        public bool NeedsVerification { get; init; }
        /// <summary>
        /// Gets or sets whether this is a patient account
        /// </summary>
        public bool IsPatient { get; init; }
        /// <summary>
        /// Gets or sets the auto-generated password
        /// Only populated for patient accounts, null for employee accounts
        /// </summary>
        public string? GeneratedPassword { get; init; }

    }
}
