namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for a service responsible for securely hashing and verifying tokens.
    /// This is typically used for storing sensitive tokens (e.g., refresh tokens) in the database 
    /// without storing the plain-text value.
    /// </summary>
    public interface ITokenHasher
    {
        /// <summary>
        /// Generates a cryptographic hash of the provided plain-text token.
        /// </summary>
        /// <param name="token">The plain-text token string to hash.</param>
        /// <returns>A secure, unreadable hash string of the token.</returns>
        string Hash(string token);

        /// <summary>
        /// Verifies a plain-text token against a stored hash to confirm a match.
        /// </summary>
        /// <param name="plainToken">The plain-text token provided by the client.</param>
        /// <param name="hashedToken">The secure hash retrieved from the database.</param>
        /// <returns>True if the plain token matches the hash; otherwise, false.</returns>
        bool Verify(string plainToken, string hashedToken);
    }
}