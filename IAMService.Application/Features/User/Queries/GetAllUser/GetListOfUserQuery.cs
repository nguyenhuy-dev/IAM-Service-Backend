using IAMService.Application.DTOs;
using MediatR;
namespace IAMService.Application.Features.User.Queries.GetAllUser
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MediatR.IRequest&lt;System.Collections.Generic.List&lt;IAMService.Application.DTOs.UserDTO&gt;&gt;" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable&lt;IAMService.Application.Features.Users.Queries.GetUsersQuery&gt;" />
    public record GetUsersQuery(
     string? SearchTerm,
     string? SortBy,
     string? SortOrder,
     int PageNumber = 1,
     int PageSize = 10
 ) : IRequest<PaginatedList<UserDto>>;

}
