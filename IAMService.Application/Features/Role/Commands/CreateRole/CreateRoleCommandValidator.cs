using FluentValidation;
namespace IAMService.Application.Features.Role.Commands.CreateRole
{
    /// <summary>
    /// The create role command validator class
    /// </summary>
    /// <seealso cref="AbstractValidator{CreateRoleCommand}"/>
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRoleCommandValidator"/> class
        /// </summary>
        public CreateRoleCommandValidator()
        {
            RuleFor(r => r.RoleName).NotNull().NotEmpty().WithMessage("RoleName is required.");
            RuleFor(r => r.RoleName).MaximumLength(50).WithMessage("RoleName must not exceed 50 characters.");

            RuleFor(r => r.RoleCode).NotNull().NotEmpty().WithMessage("RoleCode is required.");
            RuleFor(r => r.RoleCode).MaximumLength(50).WithMessage("RoleCode must not exceed 50 characters.");

            RuleFor(r => r.Description).NotNull().NotEmpty().WithMessage("Description is required.");
            RuleFor(r => r.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
        }
    }
}
