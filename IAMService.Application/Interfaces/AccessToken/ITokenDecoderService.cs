namespace IAMService.Application.Interfaces.AccessToken
{
    /// <summary>
    ///     Defines a service for decoding and extracting information from a security token (e.g., a JWT).
    /// </summary>
    public interface ITokenDecoderService
    {
        /// <summary>
        ///     Retrieves the unique identifier (e.g., 'jti' claim) from the provided token string.
        /// </summary>
        /// <param name="token">The token string (e.g., Access Token) to decode.</param>
        /// <returns>The token's unique identifier as a string.</returns>
        public string GetTokenIdentifier(string token);

        /// <summary>
        ///     Calculates the time remaining until the token expires.
        /// </summary>
        /// <param name="token">The token string (e.g., Access Token) to decode.</param>
        /// <returns>A <see cref="TimeSpan" /> representing the remaining valid duration of the token.</returns>
        public TimeSpan GetRemainingExpirationTime(string token);
    }
}
