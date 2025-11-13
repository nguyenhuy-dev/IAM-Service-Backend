using MediatR;
namespace IAMService.Application.Features.ForgotPassword.Commands
{
    /// <summary>
    ///     Command to initiate the password recovery process by requesting a reset email.
    /// </summary>
    public class ForgotPasswordCommand : IRequest<bool>
    {
        /// <summary>
        ///     Gets or sets the email address of the user requesting password reset.
        /// </summary>
        public string Email { get; set; }
    }
}
