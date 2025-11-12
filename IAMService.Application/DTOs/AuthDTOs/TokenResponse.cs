namespace IAMService.Application.DTOs.AuthDTOs
{
    /// <summary>
    ///     Token response containing access token, refresh token, and user information.
    /// </summary>
    public class TokenResponse
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="TokenResponse" /> class.
        /// </summary>
        public TokenResponse(string accessToken, int expiresInSeconds, string refreshToken, UserDto user)
        {
            AccessToken = accessToken;
            ExpiresInSeconds = expiresInSeconds;
            RefreshToken = refreshToken;
            User = user;
        }
        /// <summary>
        ///     Gets the access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        ///     Gets the access token expiration time in seconds.
        /// </summary>
        public int ExpiresInSeconds { get; set; }

        /// <summary>
        ///     Gets the refresh token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        ///     Gets the user information.
        /// </summary>
        public UserDto User { get; set; }
    }
}
