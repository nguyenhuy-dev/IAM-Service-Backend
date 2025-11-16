using MediatR;
namespace IAMService.Application.Interfaces.Events
{
    public class IntegrationEvent : INotification
    {

        public IntegrationEvent()
        {
            EventId = Guid.NewGuid();
            EventCreationDate = DateTime.UtcNow;
        }
        public Guid EventId { get; private set; }

        public DateTime EventCreationDate { get; private set; }
    }
}
