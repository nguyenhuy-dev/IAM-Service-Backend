namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for services responsible for generating various types of authentication tokens.
    /// </summary>
    public interface ITokenGenerator
    {
        /// <summary>
        /// Generates a new JSON Web Token (JWT) access token with embedded user claims.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to be included in the token claims.</param>
        /// <param name="roles">A list of roles for the user to be included in the token claims.</param>
        /// <returns>
        /// A tuple containing the generated **Access Token** string and the token's **Expiration Time in Seconds**.
        /// </returns>
        (string Token, int ExpiresInSeconds) GenerateAccessToken(Guid userId, List<string> roles);

        /// <summary>
        /// Generates a unique, cryptographically secure string to be used as a refresh token.
        /// </summary>
        /// <returns>A unique string representing the refresh token.</returns>
        string GenerateRefreshTokenString();
    }
}