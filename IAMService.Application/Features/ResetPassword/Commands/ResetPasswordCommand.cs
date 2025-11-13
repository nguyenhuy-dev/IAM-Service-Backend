using MediatR;
namespace IAMService.Application.Features.ResetPassword.Commands
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
