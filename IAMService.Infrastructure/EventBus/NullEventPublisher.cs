using IAMService.Application.Interfaces.EventBus;
using IAMService.Application.Interfaces.Events;
using Microsoft.Extensions.Logging;
namespace IAMService.Infrastructure.EventBus
{
    /// <summary>
    ///     Null Event Publisher.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.EventBus.IEventPublisher" />
    public sealed class NullEventPublisher : IEventPublisher
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NullEventPublisher" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public NullEventPublisher(ILogger<NullEventPublisher> logger)
        {
            logger.LogInformation("NullEventPublisher is used.");
        }

        /// <summary>
        ///     Publishes the asynchronous.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
        {
            return Task.FromResult(true);
        }
    }
}
