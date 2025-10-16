using AutoMapper;
using FluentValidation.Results;
using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
namespace IAMService.Application.Features.Role.Commands.CreateRole
{
    /// <summary>
    /// The create role command handler class
    /// </summary>
    /// <seealso cref="RoleDto"/>
    public class CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IPrivilegeRepository privilegeRepository,
        IMapper mapper)
        : IRequestHandler<CreateRoleCommand, RoleDto>
    {

        /// <summary>
        /// Handles the create role command.
        /// </summary>
        /// <param name="request">The create role command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created role.</returns>
        /// <exception cref="InvalidOperationException">Thrown when role code or name already exists, or when privilege IDs are invalid.</exception>
        public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var failures = new List<ValidationFailure>();

            // Validate role code uniqueness
            if (await roleRepository.ExistsByCodeAsync(request.RoleCode))
            {
                var failure = new ValidationFailure(nameof(request.RoleCode), $"Role with code '{request.RoleCode}' already exists.");
                failures.Add(failure);
            }

            // Validate role name uniqueness
            if (await roleRepository.ExistsByNameAsync(request.RoleName))
            {
                var failure = new ValidationFailure(nameof(request.RoleName), $"Role with name '{request.RoleName}' already exists.");
                failures.Add(failure);
            }

            // Validate privileges exist if any are provided
            var privilegeIds = new List<int>();
            if (request.PrivilegeIds?.ToList().Count <= 0)
            {
                // Add default if PrivilegeIds in request is empty
                privilegeIds.Add(1);
            }
            else
            {
                // Add all PrivilegeIds if not empty
                privilegeIds.AddRange(request.PrivilegeIds!);
            }
            
            if (privilegeIds.Count != 0 && !await privilegeRepository.AllExistAsync(privilegeIds))
            {
                var failure = new ValidationFailure(nameof(request.PrivilegeIds), "One or more privilege IDs are invalid.");
                failures.Add(failure);
            }

            if (failures.Count != 0)
                throw new FluentValidation.ValidationException(failures);

            // Create the role entity
            var newRole = new Domain.Entities.Role(
                0,
                request.RoleName,
                request.RoleCode,
                request.Description
            );

            // Create role with associated privileges
            var createdRole = await roleRepository.CreateAsync(newRole, privilegeIds);

            var roleDto = mapper.Map<RoleDto>(createdRole);

            return roleDto;
        }
    }
}
