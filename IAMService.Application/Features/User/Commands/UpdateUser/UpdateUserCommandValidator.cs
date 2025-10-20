using FluentValidation;
namespace IAMService.Application.Features.User.Commands.UpdateUser
{
    /// <summary>
    /// The update user command validator class.
    /// </summary>
    /// <seealso cref="AbstractValidator{UpdateUserCommand}"/>
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserCommandValidator"/> class.
        /// </summary>
        public UpdateUserCommandValidator()
        {
            // UserId required
            RuleFor(u => u.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            // FullName validation
            RuleFor(u => u.Dto.FullName)
                .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.")
                .When(u => !string.IsNullOrWhiteSpace(u.Dto.FullName));

            // Email validation
            RuleFor(u => u.Dto.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .When(u => !string.IsNullOrWhiteSpace(u.Dto.Email));

            // PhoneNumber validation (Vietnam: 10 digits)
            RuleFor(u => u.Dto.PhoneNumber)
                .Matches(@"^(0|\+84)\d{9}$").WithMessage("Phone number must be 10 digits and start with 0 or +84.")
                .When(u => !string.IsNullOrWhiteSpace(u.Dto.PhoneNumber));

            // Gender validation
            RuleFor(u => u.Dto.Gender)
                .NotNull().WithMessage("Gender must be provided.")
                .When(u => u.Dto.Gender.HasValue);

            // DateOfBirth format (MM/dd/yyyy)
            RuleFor(u => u.Dto.DateOfBirth)
                .Matches(@"^(0[1-9]|1[0-2])\/(0[1-9]|[12][0-9]|3[01])\/\d{4}$")
                .WithMessage("DateOfBirth must be in MM/DD/YYYY format.")
                .When(u => u.Dto.DateOfBirth != null);

            // IdentityNumber validation (9–12 digits)
            RuleFor(u => u.Dto.IdentityNumber)
                .Matches(@"^\d{9,12}$").WithMessage("IdentityNumber must contain 9 to 12 digits.")
                .When(u => !string.IsNullOrWhiteSpace(u.Dto.IdentityNumber));

            // Address validation
            RuleFor(u => u.Dto.Address)
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters.")
                .When(u => !string.IsNullOrWhiteSpace(u.Dto.Address));

            // At least one field must be provided
            RuleFor(u => u.Dto)
                .Must(dto =>
                    dto.FullName != null ||
                    dto.PhoneNumber != null ||
                    dto.Email != null ||
                    dto.Gender != null ||
                    dto.IdentityNumber != null ||
                    dto.DateOfBirth != null ||
                    dto.Address != null ||
                    (dto.PrivilegeIds != null && dto.PrivilegeIds.Any()))
                .WithMessage("At least one field must be provided for update.");

            //  Privilege rule — only admin can modify privileges
            When(u => u.Dto.PrivilegeIds != null && u.Dto.PrivilegeIds.Any(), () =>
            {
                RuleFor(u => u.IsAdmin)
                    .Equal(true)
                    .WithMessage("Only admin users can modify privileges for other users.");
            });
        }
    }
}
