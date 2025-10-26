using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Application.Interfaces.Messaging;

namespace IAMService.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Refresh token command handler.
/// </summary>
/// <seealso cref="IAMService.Application.Interfaces.Messaging.ICommandHandler&lt;IAMService.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand, IAMService.Application.DTOs.AuthDTOs.RefreshTokenResponse&gt;" />
public class RefreshTokenCommandHandler(IAuthRepository authRepository, IUserTokenGenerator userTokenGenerator, IUnitOfWork unitOfWork) : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    /// <summary>
    /// The authentication repository
    /// </summary>
    private readonly IAuthRepository _authRepository = authRepository;

    /// <summary>
    /// The user token generator
    /// </summary>
    private readonly IUserTokenGenerator _userTokenGenerator = userTokenGenerator;

    /// <summary>
    /// The unit of work
    /// </summary>
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">Delete all related tokens unsuccessfully!</exception>
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _authRepository.GetUserWithOldAccessToken(request.AccessToken, cancellationToken);
        
        var countDelete = await _authRepository.DeleteAllJwtTokens(user.UserId, cancellationToken);
        if (countDelete == 0)
            throw new InvalidOperationException("Delete all related tokens unsuccessfully!");

        var jwtToken = await _userTokenGenerator.GenerateTokenAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse { AccessToken = jwtToken.AccessToken };
    }
}
