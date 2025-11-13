using MediatR;
namespace IAMService.Application.Interfaces.Messaging
{
    /// <summary>
    ///     Interface for command.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="MediatR.IRequest&lt;TResponse&gt;" />
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}
