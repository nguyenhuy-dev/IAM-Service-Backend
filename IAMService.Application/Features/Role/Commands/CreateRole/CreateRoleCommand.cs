using IAMService.Application.DTOs;
using MediatR;
namespace IAMService.Application.Features.Role.Commands.CreateRole
{
    /// <summary>
    /// The create role command
    /// </summary>
    public record CreateRoleCommand(
        string RoleName,
        string RoleCode,
        string Description,
        IEnumerable<int> PrivilegeIds
    ) : IRequest<RoleDto>;
}
