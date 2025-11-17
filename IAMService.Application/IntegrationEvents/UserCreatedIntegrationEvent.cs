using IAMService.Application.Interfaces.Events;
namespace IAMService.Application.IntegrationEvents
{
    public class UserCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;

        public string Email { get; set; } = default!;

        public bool Gender { get; set; }

        public string IdentityNumber { get; set; } = default!;

        public DateOnly DateOfBirth { get; set; }

        public string Address { get; set; } = default!;
    }
}
