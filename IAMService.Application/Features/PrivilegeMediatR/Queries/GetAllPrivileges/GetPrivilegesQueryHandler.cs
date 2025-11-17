using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.Messaging;
using IAMService.Domain.Entities;
namespace IAMService.Application.Features.PrivilegeMediatR.Queries.GetAllPrivileges
{
    /// <summary>
    ///     Get Privileges Handler.
    /// </summary>
    /// <seealso
    ///     cref="IAMService.Application.Interfaces.Messaging.IQueryHandler&lt;IAMService.Application.Features.PrivilegeMediatR.Queries.GetAllPrivileges.GetPrivilegesQuery, System.Collections.Generic.List&lt;IAMService.Domain.Entities.Privilege&gt;&gt;" />
    public class GetPrivilegesQueryHandler(IPrivilegeRepository privilegeRepository) : IQueryHandler<GetPrivilegesQuery, List<Privilege>>
    {
        /// <summary>
        ///     The privilege repository
        /// </summary>
        private readonly IPrivilegeRepository _privilegeRepository = privilegeRepository;

        /// <summary>
        ///     Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<Privilege>> Handle(GetPrivilegesQuery request, CancellationToken cancellationToken)
        {
            var privileges = await _privilegeRepository.GetPrivilegesIncludeRolesAsync(cancellationToken);

            return [..privileges];
        }
    }
}
