using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Exceptions;
using IAMService.Application.Interfaces;
using MediatR;
namespace IAMService.Application.Features.Role.Commands.UpdateRole
{
    /// <summary>
    /// The update role command handler class
    /// </summary>
    /// <seealso cref="IRequestHandler{UpdateRoleCommand, RoleDto}"/>
    public class UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        IMapper mapper)
        : IRequestHandler<UpdateRoleCommand, RoleDto>
    {

        /// <summary>
        /// Handles the request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="FluentValidation.ValidationException"></exception>
        /// <exception cref="NotFoundException">Role Id not found </exception>
        /// <returns>A task containing the role dto</returns>
        public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var existingRole = await roleRepository.GetByIdAsync(request.RoleId);
            if (existingRole == null)
            {
                throw new NotFoundException("RoleId", request.RoleId);
            }

            // Update entity
            existingRole.UpdateRoleCode(request.RoleCode);
            existingRole.UpdateRoleName(request.RoleName);
            existingRole.UpdateDescription(request.Description);

            // Handle privilege IDs
            var privilegeIds = request.PrivilegeIds?.Any() == true 
                ? request.PrivilegeIds.ToList() 
                : [1];

            var updatedRole = await roleRepository.UpdateAsync(existingRole, privilegeIds);

            return mapper.Map<RoleDto>(updatedRole);
        }
    }
}
