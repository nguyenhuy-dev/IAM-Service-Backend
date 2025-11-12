using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
namespace IAMService.Application.Features.Role.Commands.CreateRole
{
    /// <summary>
    ///     The create role command handler class
    /// </summary>
    /// <seealso cref="RoleDto" />
    public class CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IMapper mapper)
        : IRequestHandler<CreateRoleCommand, RoleDto>
    {

        /// <summary>
        ///     Handles the create role command.
        /// </summary>
        /// <param name="request">The create role command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created role.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when role code or name already exists, or when privilege IDs are
        ///     invalid.
        /// </exception>
        public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            // Create the role entity
            var newRole = new Domain.Entities.Role(
                0,
                request.RoleName,
                request.RoleCode,
                request.Description
            );

            // Handle privilege IDs
            var privilegeIds = request.PrivilegeIds?.Any() == true
                ? request.PrivilegeIds.ToList()
                : [1];

            // Create role with associated privileges
            var createdRole = await roleRepository.CreateAsync(newRole, privilegeIds);

            var roleDto = mapper.Map<RoleDto>(createdRole);

            return roleDto;
        }
    }
}
