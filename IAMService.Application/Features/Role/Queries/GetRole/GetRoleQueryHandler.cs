using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
using System.Linq.Expressions;
namespace IAMService.Application.Features.Role.Queries.GetRole
{
    /// <summary>
    /// </summary>
    /// <seealso
    ///     cref="MediatR.IRequestHandler&lt;IAMService.Application.Features.Role.Queries.GetRole.GetRoleQuery, IAMService.Application.DTOs.PaginatedList&lt;IAMService.Application.DTOs.GetRoleRequest&gt;&gt;" />
    public class GetRoleQueryHandler : IRequestHandler<GetRoleQuery, PaginatedList<GetRoleRequest>>
    {
        /// <summary>
        ///     The mapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        ///     The role repository
        /// </summary>
        private readonly IRoleRepository _roleRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetRoleQueryHandler" /> class.
        /// </summary>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="mapper">The mapper.</param>
        public GetRoleQueryHandler(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        /// <summary>
        ///     Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        ///     Response from the request
        /// </returns>
        public async Task<PaginatedList<GetRoleRequest>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
        {
            // 1. Get the base queryable source
            var rolesQueryable = _roleRepository.GetRoleWithPrivileges();

            // 2. Apply filtering
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                rolesQueryable = rolesQueryable.Where(r =>
                    r.RoleName.ToLower().Contains(request.SearchTerm.ToLower()) ||
                    r.RoleCode.ToLower().Contains(request.SearchTerm.ToLower()) ||
                    r.Description != null && r.Description.ToLower().Contains(request.SearchTerm.ToLower()));
            }

            // 3. Apply simple sorting
            //Valid options: RoleName, RoleCode, RoleId. Defaults to RoleId
            Expression<Func<Domain.Entities.Role, object>> keySelector = request.SortBy?.ToLower() switch
            {
                "rolecode" => role => role.RoleCode,
                "rolename" => role => role.RoleName,
                _ => role => role.RoleId
            };
            //Valid options: asc, desc. Defaults to asc
            if (request.SortOrder?.ToLower() == "desc")
            {
                rolesQueryable = rolesQueryable.OrderByDescending(keySelector);
            }
            else
            {
                rolesQueryable = rolesQueryable.OrderBy(keySelector);
            }

            var dtoQueryable = _mapper.ProjectTo<GetRoleRequest>(rolesQueryable);

            // 5. Create the paginated list
            return await PaginatedList<GetRoleRequest>.CreateAsync(dtoQueryable, request.PageNumber, request.PageSize);
        }
    }
}
