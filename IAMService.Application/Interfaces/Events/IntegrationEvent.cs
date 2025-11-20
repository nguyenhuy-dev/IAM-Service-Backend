using MediatR;
namespace IAMService.Application.Interfaces.Events
{
    /// <summary>
    ///     Integration event base class.
    /// </summary>
    /// <seealso cref="MediatR.INotification" />
    public class IntegrationEvent : INotification
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntegrationEvent" /> class.
        /// </summary>
        public IntegrationEvent()
        {
            EventId = Guid.NewGuid();
            EventCreationDate = DateTime.UtcNow;
        }
        /// <summary>
        ///     Gets the event identifier.
        /// </summary>
        /// <value>
        ///     The event identifier.
        /// </value>
        public Guid EventId { get; private set; }

        /// <summary>
        ///     Gets the event creation date.
        /// </summary>
        /// <value>
        ///     The event creation date.
        /// </value>
        public DateTime EventCreationDate { get; private set; }
    }
}
