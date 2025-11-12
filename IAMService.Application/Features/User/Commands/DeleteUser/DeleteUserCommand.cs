using MediatR;
namespace IAMService.Application.Features.User.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
}
