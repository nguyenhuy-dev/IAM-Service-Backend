using IAMService.Application.DTOs;
using MediatR;
namespace IAMService.Application.Features.Role.Queries.GetRole
{
    /// <summary>
    ///     Query to get roles with optional filtering, sorting, and pagination
    /// </summary>
    /// <seealso
    ///     cref="MediatR.IRequest&lt;IAMService.Application.DTOs.PaginatedList&lt;IAMService.Application.DTOs.GetRoleRequest&gt;&gt;" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable&lt;IAMService.Application.Features.Role.Queries.GetRole.GetRoleQuery&gt;" />
    public record GetRoleQuery(
        string? SearchTerm,
        string? SortBy,
        string? SortOrder,
        int PageNumber = 1,
        int PageSize = 10) : IRequest<PaginatedList<GetRoleRequest>>;
}
