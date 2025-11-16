using MediatR;
namespace IAMService.Application.Interfaces.Messaging
{
    /// <summary>
    ///     Interface for Query Handler.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="MediatR.IRequestHandler&lt;TQuery, TResponse&gt;" />
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
    }
}
