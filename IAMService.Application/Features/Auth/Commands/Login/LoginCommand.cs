using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Interfaces.Messaging;
namespace IAMService.Application.Features.Auth.Commands.Login
{
    /// <summary>
    ///     Login command.
    /// </summary>
    /// <seealso
    ///     cref="IAMService.Application.Interfaces.Messaging.ICommand&lt;IAMService.Application.DTOs.AuthDTOs.LoginResponse&gt;" />
    /// <seealso cref="MediatR.IRequest&lt;IAMService.Application.DTOs.AuthDTOs.LoginResponse&gt;" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable&lt;IAMService.Application.Features.Auth.Commands.Login.LoginCommand&gt;" />
    public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;
}
