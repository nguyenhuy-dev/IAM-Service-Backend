using IAMService.Application.DTOs.Auth.Login;
using IAMService.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IAMService.Application.Features.Login.Commands
{
    /// <summary>
    /// Handles the <see cref="LoginCommand"/> by using the authentication service
    /// to validate credentials and generate tokens.
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponse>
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
        /// </summary>
        /// <param name="authService">The service responsible for core authentication logic and token generation.</param>
        public LoginCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Handles the login command, calling the authentication service to log in the user.
        /// </summary>
        /// <param name="command">The login command containing the user's email and password.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TokenResponse"/> containing the access token, refresh token, and user details.</returns>
        public async Task<TokenResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            // Assuming IAuthService.LoginAsync returns an object (let's call it LoginResult) 
            // that contains AccessToken, RefreshToken, ExpiresInSeconds, and User.
            var loginResult = await _authService.LoginAsync(command.Email, command.Password);

            // NOTE: I've added mapping for ExpiresInSeconds here, as it was missing in your original DTO mapping.
            return new TokenResponse
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken,
                ExpiresInSeconds = loginResult.ExpiresInSeconds, // This needs to be mapped!
                User = loginResult.User,
            };
        }
    }
}