namespace IAMService.Application.Interfaces.EventBus
{
    /// <summary>
    ///     Message envelop support event bus message wrapping.
    /// </summary>
    public class MessageEnvelop
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageEnvelop" /> class.
        /// </summary>
        public MessageEnvelop() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageEnvelop" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="message">The message.</param>
        public MessageEnvelop(Type type, string message) : this(type.FullName!, message) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageEnvelop" /> class.
        /// </summary>
        /// <param name="messageTypeName">Name of the message type.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     messageTypeName
        ///     or
        ///     message
        /// </exception>
        public MessageEnvelop(string messageTypeName, string message)
        {
            MessageTypeName = messageTypeName ?? throw new ArgumentNullException(nameof(messageTypeName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
        /// <summary>
        ///     Gets or sets the name of the message type.
        /// </summary>
        /// <value>
        ///     The name of the message type.
        /// </value>
        public string MessageTypeName { get; set; } = default!;

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message { get; set; } = default!;
    }
}
