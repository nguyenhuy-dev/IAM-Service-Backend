using IAMService.Application.DTOs;
using MediatR;
namespace IAMService.Application.Features.Role.Commands.UpdateRole
{
    /// <summary>
    /// The update role command
    /// </summary>
    public record UpdateRoleCommand(
        int RoleId,
        string RoleName,
        string RoleCode,
        string Description,
        IEnumerable<int> PrivilegeIds
    ) : IRequest<RoleDto>;
}
