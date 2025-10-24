using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Application.Interfaces.Messaging;
using Mapster;

namespace IAMService.Application.Features.Auth.Commands.Login;

/// <summary>
/// Login command handler.
/// </summary>
/// <seealso cref="IAMService.Application.Interfaces.Messaging.ICommandHandler&lt;IAMService.Application.Features.Auth.Commands.Login.LoginCommand, IAMService.Application.DTOs.AuthDTOs.LoginResponse&gt;" />
public class LoginCommandHandler(IAuthRepository authRepository, IUserTokenGenerator userTokenGenerator, 
    IUnitOfWork unitOfWork) : ICommandHandler<LoginCommand, LoginResponse>
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
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _authRepository.Login(request.Email, request.Password, cancellationToken);

        await _authRepository.DeleteAllJwtTokens(user.UserId, cancellationToken);

        var userToken = await _userTokenGenerator.GenerateTokenAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loginResponse = user.Adapt<LoginResponse>();
        loginResponse.RoleCode = user.Role.RoleCode;
        loginResponse.AccessToken = userToken.AccessToken;

        return loginResponse;
    }
}
