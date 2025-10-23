namespace IAMService.Application.DTOs.Auth.Login
{
    /// <summary>
    /// Represents the response object returned after a successful user login.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// The JSON Web Token (JWT) used for accessing protected resources.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// The token used to obtain a new access token without requiring re-authentication.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// The duration, in seconds, until the access token expires.
        /// </summary>
        public int ExpiresInSeconds { get; set; }

        /// <summary>
        /// Essential details about the authenticated user.
        /// </summary>
        public UserDto User { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenResponse"/> class.
        /// </summary>
        public TokenResponse() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenResponse"/> class with specified values.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="expiresInSeconds">The token expiration time in seconds.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="user">The authenticated user's details.</param>
        public TokenResponse(string accessToken, int expiresInSeconds, string refreshToken, UserDto user)
        {
            AccessToken = accessToken;
            ExpiresInSeconds = expiresInSeconds;
            RefreshToken = refreshToken;
            User = user;
        }
    }
}