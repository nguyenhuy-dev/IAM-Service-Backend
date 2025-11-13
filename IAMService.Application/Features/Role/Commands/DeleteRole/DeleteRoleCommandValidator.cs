using FluentValidation;
using IAMService.Application.Interfaces;
namespace IAMService.Application.Features.Role.Commands.DeleteRole
{
    /// <summary>
    ///     The validator for <see cref="DeleteRoleCommand" />.
    /// </summary>
    public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
    {
        /// <summary>
        ///     The read only code
        /// </summary>
        private const string ReadOnlyCode = "ReadOnly";
        /// <summary>
        ///     The role repository
        /// </summary>
        private readonly IRoleRepository _roleRepository;
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeleteRoleCommandValidator" /> class.
        /// </summary>
        /// <param name="roleRepository">The role repository.</param>
        public DeleteRoleCommandValidator(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
            RuleFor(r => r.RoleId)
                .Cascade(CascadeMode.Stop)
                .GreaterThan(0).WithMessage("RoleId must be greater than zero.")
                .NotEmpty().WithMessage("RoleId is required.")
                .CustomAsync(ValidateRoleAsync);
        }
        /// <summary>
        ///     Validates all role-related business rules in a single database call.
        /// </summary>
        private async Task ValidateRoleAsync(
            int roleId,
            ValidationContext<DeleteRoleCommand> context,
            CancellationToken cancellationToken)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);

            // Check if role exists
            if (role == null)
            {
                context.AddFailure("RoleId", "Role not found.");
                return;
            }

            // Check if it's a default role
            if (role.IsDefault)
            {
                context.AddFailure("RoleId", "Default roles cannot be deleted.");
                return;
            }

            // Check if it's the ReadOnly role
            if (role.RoleCode == ReadOnlyCode)
            {
                context.AddFailure("RoleId", "The ReadOnly role cannot be deleted.");
            }
        }
    }
}
