using MediatR;

namespace IAMService.Application.Interfaces.Messaging;

/// <summary>
/// Interface for command handler.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <seealso cref="MediatR.IRequestHandler&lt;TCommand, TResponse&gt;" />
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
}
