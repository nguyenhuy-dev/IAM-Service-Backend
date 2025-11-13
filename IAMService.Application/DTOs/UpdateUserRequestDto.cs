using System.ComponentModel.DataAnnotations;
namespace IAMService.Application.DTOs
{
    /// <summary>
    ///     The update user request class
    /// </summary>
    public class UpdateUserRequestDto
    {
        /// <summary>
        ///     Gets or sets the full name.
        /// </summary>
        /// <value>
        ///     The full name.
        /// </value>
        public string? FullName { get; set; }
        /// <summary>
        ///     Gets or sets the phone number.
        /// </summary>
        /// <value>
        ///     The phone number.
        /// </value>
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }
        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        /// <value>
        ///     The email.
        /// </value>
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
        /// <summary>
        ///     Gets or sets the gender.
        /// </summary>
        /// <value>
        ///     The gender.
        /// </value>
        public bool? Gender { get; set; }
        /// <summary>
        ///     Gets or sets the identity number.
        /// </summary>
        /// <value>
        ///     The identity number.
        /// </value>
        public string? IdentityNumber { get; set; }
        /// <summary>
        ///     Gets or sets the date of birth.
        /// </summary>
        /// <value>
        ///     The date of birth.
        /// </value>
        [RegularExpression(@"^(0[1-9]|1[0-2])/(0[1-9]|[12][0-9]|3[01])/\d{4}$", ErrorMessage = "Date of birth must be in MM/DD/YYYY format.")]
        public string? DateOfBirth { get; set; }
        /// <summary>
        ///     Gets or sets the address.
        /// </summary>
        /// <value>
        ///     The address.
        /// </value>
        public string? Address { get; set; }

        /// <summary>
        ///     Gets or sets the privilege ids.
        /// </summary>
        /// <value>
        ///     The privilege ids.
        /// </value>
        public List<int>? PrivilegeIds { get; set; }
    }
}
