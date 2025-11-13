using IAMService.Application.DTOs;
using MediatR;
namespace IAMService.Application.Features.User.Commands.CreateUser
{
    /// <summary>
    ///     Command to create a new user account
    ///     Implements CQRS pattern using MediatR
    /// </summary>
    public record CreateUserCommand : IRequest<UserDto>
    {
        /// <summary>
        ///     Gets the user's email address
        ///     Must be in valid email format
        /// </summary>
        public required string Email { get; init; }
        /// <summary>
        ///     Gets the user's phone number
        ///     Must be in valid phone format
        /// </summary>
        public required string PhoneNumber { get; init; }
        /// <summary>
        ///     Gets the user's full name
        /// </summary>
        public required string FullName { get; init; }
        /// <summary>
        ///     Gets the user's identity number (ID card, passport, etc.)
        ///     Must be in valid format
        /// </summary>
        public required string IdentityNumber { get; init; }
        /// <summary>
        ///     Gets the user's gender
        ///     Valid values: "Male", "Female"
        /// </summary>
        public required string Gender { get; init; }
        /// <summary>
        ///     Gets the user's address
        /// </summary>
        public required string Address { get; init; }
        /// <summary>
        ///     Gets the user's date of birth
        ///     Must be in MM/DD/YYYY format
        /// </summary>
        public required string DateOfBirth { get; init; }
        /// <summary>
        ///     Gets the user's password
        ///     Only required for employee accounts
        ///     Must be a strong password
        ///     For patient accounts, this will be auto-generated
        /// </summary>
        public string? Password { get; init; }
        /// <summary>
        ///     Gets whether this user is a patient
        ///     True = patient account (password auto-generated)
        ///     False = employee account (password required)
        /// </summary>
        public bool IsPatient { get; init; }
        /// <summary>
        ///     Gets the privilege IDs to assign to the user
        ///     If empty, default Read-Only privilege (ID=1) will be assigned
        /// </summary>
        public IEnumerable<int>? PrivilegeIds { get; init; }
    }
}
