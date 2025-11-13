using FluentValidation;
using IAMService.Application.Exceptions;
using IAMService.Application.Interfaces;
using MediatR;
namespace IAMService.Application.Features.Role.Commands.DeleteRole
{
    /// <summary>
    ///     The handler for deleting a role command.
    /// </summary>
    /// <seealso
    ///     cref="MediatR.IRequestHandler&lt;IAMService.Application.Features.Role.Commands.DeleteRole.DeleteRoleCommand&gt;" />
    public class DeleteRoleCommandHandler(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteRoleCommand, bool>
    {
        private const string ReadOnlyCode = "READ_ONLY";
        /// <summary>
        ///     The read only code
        /// </summary>
        /// <summary>
        ///     Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="IAMService.Application.Exceptions.NotFoundException">RoleId</exception>
        /// <exception cref="FluentValidation.ValidationException">Default roles cannot be deleted.</exception>
        public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var roleToDelete = await roleRepository.GetByIdAsync(request.RoleId);
            if (roleToDelete == null)
            {
                throw new NotFoundException("RoleId", request.RoleId);
            }
            if (roleToDelete.IsDefault)
            {
                throw new ValidationException("Default roles cannot be deleted.");
            }
            if (roleToDelete.RoleCode == ReadOnlyCode)
            {
                throw new ValidationException("The ReadOnly role cannot be deleted.");
            }
            var usersToUpdate = await userRepository.GetByRoleIdAsync(request.RoleId);
            var readOnlyRole = await roleRepository.GetByCodeAsync(ReadOnlyCode);
            if (readOnlyRole == null)
            {
                throw new InvalidOperationException($"System configuration error: The '{ReadOnlyCode}' role was not found.");
            }
            foreach (var user in usersToUpdate)
            {
                user.RoleId = readOnlyRole.RoleId;
            }
            userRepository.UpdateRange(usersToUpdate);
            await roleRepository.DeleteAsync(roleToDelete);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
