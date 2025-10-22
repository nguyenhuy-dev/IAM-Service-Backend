using IAMService.Application.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace IAMService.Infrastructure.Services
{
    /// <summary>
    /// Implements the <see cref="ITokenHasher"/> contract using SHA256 for one-way hashing of tokens
    /// (e.g., Refresh Tokens) for secure storage and efficient lookup in the database.
    /// </summary>
    /// <remarks>
    /// NOTE: SHA256 is a fast cryptographic hash, suitable for unique token identification and integrity checking, 
    /// but NOT suitable for password hashing, which requires slow, salted, adaptive functions like Argon2 or PBKDF2.
    /// </remarks>
    public class TokenHasher : ITokenHasher
    {
        /// <summary>
        /// Generates a cryptographic hash of the provided plain-text token using SHA256.
        /// </summary>
        /// <param name="token">The plain-text token string (e.g., the opaque Refresh Token) to hash.</param>
        /// <returns>A Base64-encoded string representing the SHA256 hash of the token.</returns>
        public string Hash(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies a plain-text token against a stored hash to confirm a match.
        /// </summary>
        /// <param name="plainToken">The plain-text token provided by the client.</param>
        /// <param name="hashedToken">The secure hash retrieved from the database.</param>
        /// <returns>True if the hash of the plain token matches the stored hash; otherwise, false.</returns>
        public bool Verify(string plainToken, string hashedToken)
        {
            var newHash = Hash(plainToken);
            return newHash == hashedToken;
        }
    }
}