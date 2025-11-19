using IAMService.Application.DTOs;
using MediatR;
namespace IAMService.Application.Features.User.Queries.GetAllUser
{
    /// <summary>
    ///     Query model for fetching and filtering users with pagination.
    /// </summary>
    public record GetUsersQuery(
        string? SearchTerm,
        string? SortBy,
        string? SortOrder,
        int PageNumber = 1,
        int PageSize = 10,
        List<string>? RoleCodes = null,
        List<string>? PrivilegeNames = null,
        bool ExcludePatients = false
    ) : IRequest<PaginatedList<UserDto>>;
}
