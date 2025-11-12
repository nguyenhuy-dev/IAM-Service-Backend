using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Application.Interfaces.Messaging;
namespace IAMService.Application.Features.Auth.Commands.Logout
{
    /// <summary>
    ///     Logout command handler.
    /// </summary>
    /// <seealso
    ///     cref="IAMService.Application.Interfaces.Messaging.ICommandHandler&lt;IAMService.Application.Features.Auth.Commands.Logout.LogoutCommand, System.Boolean&gt;" />
    public class LogoutCommandHandler(IAuthRepository authRepository, IUnitOfWork unitOfWork) : ICommandHandler<LogoutCommand, bool>
    {
        /// <summary>
        ///     The authentication repository
        /// </summary>
        private readonly IAuthRepository _authRepository = authRepository;

        /// <summary>
        ///     The unit of work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        ///     Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Delete all related tokens unsuccessfully!</exception>
        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var countDelete = await _authRepository.DeleteAllJwtTokens(request.UserId, cancellationToken);
            if (countDelete == 0)
                throw new InvalidOperationException("Delete all related tokens unsuccessfully!");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
