using MediatR;
namespace IAMService.Application.Interfaces.Messaging
{
    /// <summary>
    ///     Interface for Query.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="MediatR.IRequest&lt;TResponse&gt;" />
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
