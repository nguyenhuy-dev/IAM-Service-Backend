using FluentValidation;
using IAMService.Application.Features.Role.Validators;
using IAMService.Application.Interfaces;
namespace IAMService.Application.Features.Role.Commands.CreateRole
{
    /// <summary>
    ///     The create role command validator class
    /// </summary>
    /// <seealso cref="AbstractValidator{CreateRoleCommand}" />
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateRoleCommandValidator" /> class
        /// </summary>
        public CreateRoleCommandValidator(
            IRoleRepository roleRepository,
            IPrivilegeRepository privilegeRepository)
        {
            RuleFor(r => r.RoleName)
                .NotEmpty().WithMessage("RoleName is required.")
                .MaximumLength(50).WithMessage("RoleName must not exceed 50 characters.")
                .MustBeUniqueRoleName(roleRepository);

            RuleFor(r => r.RoleCode)
                .NotEmpty().WithMessage("RoleCode is required.")
                .MaximumLength(50).WithMessage("RoleCode must not exceed 50 characters.")
                .MustBeUniqueRoleCode(roleRepository);

            RuleFor(r => r.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

            RuleFor(r => r.PrivilegeIds)
                .MustHaveValidPrivileges(privilegeRepository)
                .When(r => r.PrivilegeIds.Any());
        }
    }
}
