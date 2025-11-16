using Grpc.Core;
using IAMService.API.gRPC.Protos.UserProto;
using IAMService.Application.Features.User.Queries.GetAllUsers;
using IAMService.Application.Interfaces;
using MediatR;
namespace IAMService.API.gRPC.Services
{
    public class UserGrpcService : UserService.UserServiceBase
    {
        private readonly ISender _sender;
        private readonly IStringEncryptionService _stringEncryptionService;

        public UserGrpcService(ISender sender, IStringEncryptionService stringEncryptionService)
        {
            _sender = sender;
            _stringEncryptionService = stringEncryptionService;
        }

        /// <summary>
        ///     Gets all users with decrypted sensitive information
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="context">The server call context</param>
        /// <returns>Response containing all users with decrypted data</returns>
        public override async Task<GetAllUsersResponse> GetAllUsers(
            GetAllUsersRequest request,
            ServerCallContext context)
        {
            var query = new GetAllUsersQuery();
            var users = await _sender.Send(query, context.CancellationToken);

            var response = new GetAllUsersResponse();

            foreach (var user in users)
            {
                // Decrypt sensitive fields
                var decryptedFullName = _stringEncryptionService.DecryptString(user.FullName ?? string.Empty);
                var decryptedEmail = _stringEncryptionService.DecryptString(user.Email ?? string.Empty);

                var userMessage = new UserMessage
                {
                    UserId = user.UserId.ToString(),
                    FullName = decryptedFullName,
                    Email = decryptedEmail,
                    IsActive = user.IsActive
                };

                response.Users.Add(userMessage);
            }

            return response;
        }
    }
}
