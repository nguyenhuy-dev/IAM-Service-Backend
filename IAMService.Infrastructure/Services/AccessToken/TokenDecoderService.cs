using IAMService.Application.Interfaces.AccessToken;
using System.IdentityModel.Tokens.Jwt;
// Added for FirstOrDefault

namespace IAMService.Infrastructure.Services.AccessToken
{
    /// <summary>
    ///     A service implementation for decoding and extracting information from a JSON Web Token (JWT),
    ///     adhering to the <see cref="ITokenDecoderService" /> interface.
    /// </summary>
    public class TokenDecoderService : ITokenDecoderService
    {
        /// <summary>
        ///     Calculates the time remaining until the token expires based on the 'exp' claim.
        /// </summary>
        /// <param name="token">The raw JWT string.</param>
        /// <returns>
        ///     A <see cref="TimeSpan" /> representing the remaining valid duration. Returns <see cref="TimeSpan.Zero" /> if
        ///     the token is invalid or already expired.
        /// </returns>
        public TimeSpan GetRemainingExpirationTime(string token)
        {
            try
            {
                var handle = new JwtSecurityTokenHandler();
                // Reads the token without validation (only parsing)
                var jwt = handle.ReadJwtToken(token);

                // 'ValidTo' property comes from the 'exp' (expiration) claim
                var expiry = jwt.ValidTo;
                var remainingTime = expiry.Subtract(DateTime.UtcNow);

                // Return remaining time or zero if it's already expired
                return remainingTime > TimeSpan.Zero ? remainingTime : TimeSpan.Zero;
            }
            catch (Exception) // Catch exceptions like invalid token format
            {
                return TimeSpan.Zero;
            }
        }


        /// <summary>
        ///     Extracts the unique identifier (JTI - JWT ID) from the claims of the token.
        /// </summary>
        /// <param name="token">The raw JWT string.</param>
        /// <returns>The JTI value as a string.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the JTI claim is not found in a valid token.</exception>
        public string GetTokenIdentifier(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                // Reads the token without validation (only parsing)
                var jwtToken = handler.ReadJwtToken(token);

                // Get the JTI (JWT ID) claim
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

                // Throw exception if the JTI claim is missing
                return jti?.Value ?? throw new InvalidOperationException("JTI claim not found in token.");
            }
            catch (Exception) // Catch exceptions like invalid token format or missing claim
            {
                return null;
            }
        }
    }
}
