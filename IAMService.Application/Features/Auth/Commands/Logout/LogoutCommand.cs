using IAMService.Application.Interfaces.Messaging;
namespace IAMService.Application.Features.Auth.Commands.Logout
{
    /// <summary>
    ///     Logout command.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.Messaging.ICommand&lt;System.Boolean&gt;" />
    /// <seealso cref="MediatR.IRequest&lt;System.Boolean&gt;" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable&lt;IAMService.Application.Features.Auth.Commands.Logout.LogoutCommand&gt;" />
    public sealed record LogoutCommand(Guid UserId) : ICommand<bool>;
}
