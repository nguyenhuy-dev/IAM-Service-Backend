namespace IAMService.Application.Interfaces
{
    /// <summary>
    ///     Interface for password hashing operations
    ///     Provides methods to hash and verify passwords securely
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        ///     Hashes a plain text password using a secure hashing algorithm
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The hashed password string</returns>
        string HashPassword(string password);
        /// <summary>
        ///     Verifies if a plain text password matches a hashed password
        /// </summary>
        /// <param name="hashedPassword">The stored hashed password</param>
        /// <param name="providedPassword">The plain text password to verify</param>
        /// <returns>True if passwords match, otherwise false</returns>
        bool VerifyPassword(string hashedPassword, string providedPassword);
        /// <summary>
        ///     Generates a random strong password
        ///     Password will contain uppercase, lowercase, digits, and special characters
        /// </summary>
        /// <param name="length">The desired length of the password</param>
        /// <returns>A randomly generated strong password</returns>
        string GenerateRandomPassword(int length = 12);
    }
}
