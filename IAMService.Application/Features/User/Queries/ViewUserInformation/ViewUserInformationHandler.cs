using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Exceptions;
using IAMService.Application.Interfaces;
using MediatR;
namespace IAMService.Application.Features.User.Queries.ViewUserInformation
{
    /// <summary>
    ///     Handler for viewing user information.
    /// </summary>
    public class ViewUserInformationHandler : IRequestHandler<ViewUserInformationQuery, UserResponseDto>
    {
        /// <summary>
        ///     The mapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        ///     The string encryption service
        /// </summary>
        private readonly IStringEncryptionService _stringEncryptionService;
        /// <summary>
        ///     The user repository
        /// </summary>
        private readonly IUserRepository _userRepository;



        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewUserInformationHandler" /> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="stringEncryptionService">The string encryption service.</param>
        public ViewUserInformationHandler(IUserRepository userRepository, IMapper mapper, IStringEncryptionService stringEncryptionService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _stringEncryptionService = stringEncryptionService;
        }

        /// <summary>
        ///     Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        ///     Response from the request
        /// </returns>
        /// <exception cref="IAMService.Application.Exceptions.NotFoundException">User not found or has been deleted.</exception>
        /// <exception cref="IAMService.Application.Exceptions.ForbiddenAccessException">
        ///     You do not have permission to view other
        ///     users' information.
        /// </exception>
        public async Task<UserResponseDto> Handle(ViewUserInformationQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.TargetUserId);

            if (user == null)
                throw new NotFoundException("User not found or has been deleted.");

            var current = request.CurrentUser;

            var isAdminOrManager = current.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                                || current.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            if (!isAdminOrManager && current.UserId != request.TargetUserId)
                throw new ForbiddenAccessException("You do not have permission to view other users' information.");

            user.FullName = _stringEncryptionService.DecryptString(user.FullName);
            user.Email = _stringEncryptionService.DecryptString(user.Email);
            user.PhoneNumber = _stringEncryptionService.DecryptString(user.PhoneNumber);
            user.IdentityNumber = _stringEncryptionService.DecryptString(user.IdentityNumber);
            user.Address = _stringEncryptionService.DecryptString(user.Address);
            var result = _mapper.Map<UserResponseDto>(user);

            return result;
        }
    }
}
