using IAMService.Application.Interfaces.Events;
namespace IAMService.Application.Interfaces.EventBus
{
    /// <summary>
    ///     Interface for Event Publisher.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        ///     Publishes the asynchronous.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent;
    }
}
