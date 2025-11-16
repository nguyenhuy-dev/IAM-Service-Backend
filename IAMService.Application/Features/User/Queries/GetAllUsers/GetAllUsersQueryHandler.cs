using IAMService.Application.Interfaces;
using MediatR;
using UserEntity=IAMService.Domain.Entities.User;

namespace IAMService.Application.Features.User.Queries.GetAllUsers
{
    /// <summary>
    ///     Handler for GetAllUsersQuery
    /// </summary>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserEntity>>
    {
        /// <summary>
        ///     The user repository
        /// </summary>
        private readonly IUserRepository _userRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetAllUsersQueryHandler" /> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        public GetAllUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        ///     Handles the GetAllUsersQuery request
        /// </summary>
        /// <param name="request">The query request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        ///     List of all users
        /// </returns>
        public async Task<IEnumerable<UserEntity>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userRepository.GetAllUsersAsync(cancellationToken);
        }
    }
}
