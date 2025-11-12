using MediatR;
namespace IAMService.Application.Features.Role.Commands.DeleteRole
{
    /// <summary>
    ///     The Delete role command
    /// </summary>
    public record DeleteRoleCommand(int RoleId) : IRequest<bool>;
}
