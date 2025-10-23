using IAMService.Application.DTOs.Auth.Login;
using MediatR;

namespace IAMService.Application.Features.Login.Commands
{
    /// <summary>
    /// Represents a command to authenticate a user and generate tokens.
    /// This command expects a <see cref="TokenResponse"/> upon successful execution.
    /// </summary>
    public class LoginCommand : IRequest<TokenResponse>
    {
        /// <summary>
        /// Gets or sets the email address of the user attempting to log in.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password of the user attempting to log in.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginCommand"/> class.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        public LoginCommand(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}