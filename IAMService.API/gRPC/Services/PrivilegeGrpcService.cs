using Grpc.Core;
using IAMService.API.gRPC.Protos;
using IAMService.Application.Features.PrivilegeMediatR.Queries.GetAllPrivileges;
using MediatR;
namespace IAMService.API.gRPC.Services
{
    /// <summary>
    ///     gRPC service for GetAllPrivileges.
    /// </summary>
    /// <seealso cref="IAMService.API.gRPC.Protos.Privilege.PrivilegeBase" />
    public class PrivilegeGrpcService(ISender sender) : Privilege.PrivilegeBase
    {
        /// <summary>
        ///     The sender
        /// </summary>
        private readonly ISender _sender = sender;

        /// <summary>
        ///     Gets all privileges.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override async Task<PrivilegeListResponse> GetAllPrivileges(Empty request, ServerCallContext context)
        {
            var privileges = await _sender.Send(new GetPrivilegesQuery());
            var response = new PrivilegeListResponse();

            response.Privileges.AddRange(privileges.Select(p => new PrivilegeResponse
            {
                PrivilegeId = p.PrivilegeId,
                PrivilegeName = p.PrivilegeName,
                Roles =
                {
                    p.Roles.Select(r => new RoleResponse
                    {
                        RoleId = r.RoleId,
                        RoleCode = r.RoleCode
                    })
                }
            }));

            return response;
        }
    }
}
