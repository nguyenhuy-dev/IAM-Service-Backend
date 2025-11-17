using Confluent.Kafka;
using IAMService.Application.Interfaces.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace IAMService.Infrastructure.EventBus.Kafka
{
    /// <summary>
    ///     Register kafka into Program.cs.
    /// </summary>
    public static class KafkaEventBusExtensions
    {
        /// <summary>
        ///     Adds the kafka producer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="connectionName">Name of the connection.</param>
        /// <returns></returns>
        public static IHostApplicationBuilder AddKafkaProducer(this IHostApplicationBuilder builder, string connectionName)
        {
            builder.AddKafkaProducer<string, MessageEnvelop>(connectionName, settings => { },
                builder =>
                {
                    builder.SetValueSerializer(new MessageEnvelopSerializer());
                }
            );

            return builder;
        }

        /// <summary>
        ///     Adds the kafka event publisher.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="topic">The topic.</param>
        /// <exception cref="System.ArgumentNullException">topic</exception>
        public static void AddKafkaEventPublisher(this IHostApplicationBuilder builder, string? topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentNullException(nameof(topic));

            builder.Services.AddTransient<IEventPublisher>(services => new KafkaEventPublisher(topic, services.GetRequiredService<IProducer<string, MessageEnvelop>>(), services.GetRequiredService<ILoggerFactory>().CreateLogger($"EventPublisher<{topic}>"))
            );
        }
    }

    /// <summary>
    ///     Message envelop support for serializer.
    /// </summary>
    /// <seealso cref="Confluent.Kafka.ISerializer&lt;IAMService.Application.Interfaces.EventBus.MessageEnvelop&gt;" />
    internal class MessageEnvelopSerializer : ISerializer<MessageEnvelop>
    {
        /// <summary>
        ///     Serialize the key or value of a <see cref="T:Confluent.Kafka.Message`2" />
        ///     instance.
        /// </summary>
        /// <param name="data">The value to serialize.</param>
        /// <param name="context">Context relevant to the serialize operation.</param>
        /// <returns>
        ///     The serialized value.
        /// </returns>
        public byte[] Serialize(MessageEnvelop data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
