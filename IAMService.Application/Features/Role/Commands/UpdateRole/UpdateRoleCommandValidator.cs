using FluentValidation;
using IAMService.Application.Features.Role.Validators;
using IAMService.Application.Interfaces;
namespace IAMService.Application.Features.Role.Commands.UpdateRole
{
    /// <summary>
    /// The update role command validator class
    /// </summary>
    /// <seealso cref="AbstractValidator{UpdateRoleCommand}"/>
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator(
            IRoleRepository roleRepository,
            IPrivilegeRepository privilegeRepository)
        {

            RuleFor(r => r.RoleId)
                .GreaterThan(0).WithMessage("RoleId must be greater than zero.");

            RuleFor(r => r.RoleName)
                .NotEmpty().WithMessage("RoleName is required.")
                .MaximumLength(50).WithMessage("RoleName must not exceed 50 characters.")
                .MustBeUniqueRoleName(roleRepository, cmd => cmd.RoleId);

            RuleFor(r => r.RoleCode)
                .NotEmpty().WithMessage("RoleCode is required.")
                .MaximumLength(50).WithMessage("RoleCode must not exceed 50 characters.")
                .MustBeUniqueRoleCode(roleRepository, cmd => cmd.RoleId);

            RuleFor(r => r.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

            RuleFor(r => r.PrivilegeIds)
                .MustHaveValidPrivileges(privilegeRepository)
                .When(r => r.PrivilegeIds.Any());
        }
    }
}
