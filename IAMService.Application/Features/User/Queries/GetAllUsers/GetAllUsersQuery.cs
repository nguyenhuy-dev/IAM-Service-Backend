using MediatR;
using UserEntity=IAMService.Domain.Entities.User;

namespace IAMService.Application.Features.User.Queries.GetAllUsers
{
    /// <summary>
    ///     Query to get all users
    /// </summary>
    public record GetAllUsersQuery : IRequest<IEnumerable<UserEntity>>;
}
