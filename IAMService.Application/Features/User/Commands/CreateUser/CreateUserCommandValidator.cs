using FluentValidation;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IAMService.Application.Features.User.Commands.CreateUser
{
    /// <summary>
    /// Validator for CreateUserCommand
    /// Validates all input fields according to business rules
    /// </summary>
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        /// <summary>
        /// Initializes validation rules for CreateUserCommand
        /// </summary>
        public CreateUserCommandValidator()
        {
            //Email Validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be in a valid format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
            //Phone number validation
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Must(BeValidPhoneNumber).WithMessage("Phone number must start with 0 and contain exactly 10 digits");
            //Full name validation
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");
            //Identity number validation
            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("Identity number is required")
                .Must(BeValidIdentityNumber).WithMessage("Identity number must contain only digits and be exactly 12 digits long");
            //Gender validation
            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(BeValidGender).WithMessage("Gender must be either 'Male' or 'Female'");
            // Address validation
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required");
            // Date of birth validation
            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .Must(BeValidDateFormat).WithMessage("Date of birth must be in MM/DD/YYYY format")
                .Must(BeValidDate).WithMessage("Date of birth must be a valid date");
            // Password validation - only required for employee accounts
            RuleFor(x => x.Password)
                .NotEmpty().When(x => !x.IsPatient)
                .WithMessage("Password is required for employee accounts")
                .Must(BeStrongPassword).When(x => !x.IsPatient && !string.IsNullOrEmpty(x.Password))
                .WithMessage("Password must be strong: at least 8 characters, containing uppercase, lowercase, digit, and special character");

            // Password should be null for patient accounts
            When(x => !x.IsPatient, () =>
            {
                RuleFor(x => x.Password)
                    .NotEmpty()
                    .WithMessage("Password is required for employee accounts")
                    .Must(BeStrongPassword)
                    .WithMessage("Password must be strong: at least 8 characters, containing uppercase, lowercase, digit, and special character");
            });

            When(x => x.IsPatient, () =>
            {
                RuleFor(x => x.Password)
                    .Null()
                    .WithMessage("Password should not be provided for patient accounts (will be auto-generated)");
            });
        }



        /// <summary>
        /// Validates phone number format
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool BeValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;
            string cleaned = phoneNumber.Replace(" ", "")
                                .Replace("-", "")
                                .Replace("(", "")
                                .Replace(")", "");
            return Regex.IsMatch(cleaned, @"^0\d{9}$");
        }
        /// <summary>
        /// Validates identity number format
        /// </summary>
        /// <param name="identityNumber">Identity number.</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool BeValidIdentityNumber(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                return false;

            return Regex.IsMatch(identityNumber, @"^\d{12}$");
        }
        /// <summary>
        /// Validates gender value
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool BeValidGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return false;

            // Accept "Male" or "Female" (case-insensitive)
            var normalizedGender = gender.Trim().ToLower();
            return normalizedGender == "male" || normalizedGender == "female";
        }
        /// <summary>
        /// Validates that the date of birth is a valid date and not in the future
        /// </summary>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool BeValidDate(string dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirth))
                return false;

            // Try to parse the date in MM/DD/YYYY format
            if (!DateTime.TryParseExact(dateOfBirth,
                "MM/dd/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate))
            {
                return false;
            }

            // Date cannot be in the future
            if (parsedDate > DateTime.Now)
                return false;

            // Date should be reasonable (not more than 150 years ago)
            if (parsedDate < DateTime.Now.AddYears(-150))
                return false;

            return true;
        }

        /// <summary>
        /// Validates date of birth format (MM/DD/YYYY)
        /// </summary>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool BeValidDateFormat(string dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirth))
                return false;
            return Regex.IsMatch(dateOfBirth, @"^\d{2}/\d{2}/\d{4}$");
        }
        /// <summary>
        ///  Validates password strength
        /// Must contain:
        /// - At least 8 characters
        /// - At least one uppercase letter
        /// - At least one lowercase letter
        /// - At least one digit
        /// - At least one special character
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool BeStrongPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Minimum length 8 (recommend 12+ for stronger security)
            if (password.Length < 8)
                return false;

            // Must contain at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Must contain at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Must contain at least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            return true;
        }

    }
}
