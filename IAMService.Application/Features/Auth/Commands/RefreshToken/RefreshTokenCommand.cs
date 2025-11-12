using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Interfaces.Messaging;
namespace IAMService.Application.Features.Auth.Commands.RefreshToken
{
    /// <summary>
    ///     Refresh token command.
    /// </summary>
    /// <seealso
    ///     cref="IAMService.Application.Interfaces.Messaging.ICommand&lt;IAMService.Application.DTOs.AuthDTOs.RefreshTokenResponse&gt;" />
    /// <seealso cref="MediatR.IRequest&lt;IAMService.Application.DTOs.AuthDTOs.RefreshTokenResponse&gt;" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable&lt;IAMService.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand&gt;" />
    public sealed record RefreshTokenCommand(string AccessToken) : ICommand<RefreshTokenResponse>;
}
