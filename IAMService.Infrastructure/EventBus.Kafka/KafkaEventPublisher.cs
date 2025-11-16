using Confluent.Kafka;
using IAMService.Application.Interfaces.EventBus;
using IAMService.Application.Interfaces.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace IAMService.Infrastructure.EventBus.Kafka
{
    /// <summary>
    ///     Implement publishing kafka event.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.EventBus.IEventPublisher" />
    public class KafkaEventPublisher(string topic, IProducer<string, MessageEnvelop> producer, ILogger logger) : IEventPublisher
    {

        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger _logger = logger;

        /// <summary>
        ///     The producer
        /// </summary>
        private readonly IProducer<string, MessageEnvelop> _producer = producer;
        /// <summary>
        ///     The topic
        /// </summary>
        private readonly string _topic = topic;

        /// <summary>
        ///     Publishes the asynchronous.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public async Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
        {
            var json = JsonSerializer.Serialize(@event, @event.GetType());
            _logger.LogInformation("Publishing event {Type} to topic {Topic}: {Event}.", @event.GetType().Name, _topic, json);

            try
            {
                await _producer.ProduceAsync(_topic, new Message<string, MessageEnvelop> { Key = @event.GetType().FullName!, Value = new MessageEnvelop(typeof(TEvent), json) }
                );

                _logger.LogInformation("Published event {@Event}", @event.EventId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event {@Event}", @event.EventId);

                return false;
            }
        }
    }
}
