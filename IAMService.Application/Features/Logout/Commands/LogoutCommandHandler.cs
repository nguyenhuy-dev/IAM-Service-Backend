using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AccessToken;
using MediatR;

namespace IAMService.Application.Features.Logout.Commands
{
    /// <summary>
    /// Handles the command to log out a user by invalidating the access token and revoking the refresh token.
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;IAMService.Application.Features.Logout.Commands.LogoutCommand, MediatR.Unit&gt;" />
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        /// <summary>
        /// The service for managing refresh tokens.
        /// </summary>
        private readonly IRefreshTokenService _refreshTokenService;
        /// <summary>
        /// The service for invalidating access tokens.
        /// </summary>
        private readonly IInvalidationService _invalidationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutCommandHandler"/> class.
        /// </summary>
        /// <param name="refreshTokenService">The refresh token service.</param>
        /// <param name="invalidationService">The invalidation service.</param>
        public LogoutCommandHandler(IRefreshTokenService refreshTokenService, IInvalidationService invalidationService)
        {
            _refreshTokenService = refreshTokenService;
            _invalidationService = invalidationService;
        }

        /// <summary>
        /// Handles the Logout command.
        /// </summary>
        /// <param name="req">The request, containing the Access Token and Refresh Token values.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see cref="Unit"/>.</returns>
        public async Task<Unit> Handle(LogoutCommand req, CancellationToken cancellationToken)
        {
            // Invalidate the short-lived access token immediately
            await _invalidationService.InvalidationAccessTokenAsync(req.AccessTokenValue);

            // Revoke the long-lived refresh token
            if (!string.IsNullOrEmpty(req.RefreshTokenValue)) // THÊM LỚP BẢO VỆ NÀY
            {
                // Chỉ gọi service nếu giá trị tồn tại
                bool isRovokeRefreshToken = await _refreshTokenService.RevokeTokenByStringAsync(req.RefreshTokenValue);
                // ... (xử lý kết quả)
            }
            return Unit.Value;
        }
    }
}