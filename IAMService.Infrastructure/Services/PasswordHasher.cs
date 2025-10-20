using IAMService.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace IAMService.Infrastructure.Services
{
    /// <summary>
    /// Implementation of password hashing using BCrypt algorithm
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// The work factor
        /// </summary>
        private const int WorkFactor = 11;
        /// <summary>
        /// Hashes a password using BCrypt algorithm
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>
        /// The hashed password string
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Password cannot be null or empty - password</exception>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("Password cannot be null or empty", nameof(password));
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }
        /// <summary>
        /// Verifies if a plain text password matches a hashed password
        /// </summary>
        /// <param name="hashedPassword">The stored hashed password</param>
        /// <param name="providedPassword">The plain text password to verify</param>
        /// <returns>
        /// True if passwords match, otherwise false
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Hashed password cannot be null or empty - hashedPassword
        /// or
        /// Provided password cannot be null or empty - providedPassword
        /// </exception>
        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                throw new ArgumentException("Hashed password cannot be null or empty", nameof(hashedPassword));

            if (string.IsNullOrWhiteSpace(providedPassword))
                throw new ArgumentException("Provided password cannot be null or empty", nameof(providedPassword));
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
        /// <summary>
        /// Generates a random strong password
        /// Password will contain uppercase, lowercase, digits, and special characters
        /// </summary>
        /// <param name="length">The desired length of the password</param>
        /// <returns>
        /// A randomly generated strong password
        /// </returns>
        /// <exception cref="System.ArgumentException">Password length must be at least 8 characters - length</exception>
        public string GenerateRandomPassword(int length = 12)
        {
            // Validate minimum length for security
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

            // Define character sets for password generation
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            // Combine all character sets
            string allChars = uppercase + lowercase + digits + special;

            // Use StringBuilder for efficient string building
            var password = new StringBuilder();

            // Use RandomNumberGenerator for cryptographically secure random numbers
            using (var rng = RandomNumberGenerator.Create())
            {
                // Ensure at least one character from each character set
                password.Append(GetRandomChar(rng, uppercase));  // At least one uppercase
                password.Append(GetRandomChar(rng, lowercase));  // At least one lowercase
                password.Append(GetRandomChar(rng, digits));     // At least one digit
                password.Append(GetRandomChar(rng, special));    // At least one special char

                // Fill remaining length with random characters from all sets
                for (int i = 4; i < length; i++)
                {
                    password.Append(GetRandomChar(rng, allChars));
                }
            }

            // Shuffle the password characters to avoid predictable patterns
            return ShuffleString(password.ToString());
        }

        /// <summary>
        /// Gets the random character.
        /// </summary>
        /// <param name="rng">The RNG.</param>
        /// <param name="chars">The chars.</param>
        /// <returns>A randomly selected character</returns>
        private char GetRandomChar(RandomNumberGenerator rng, string chars)
        {
            // Create byte array to store random number
            byte[] randomBytes = new byte[4];

            // Fill with cryptographically random bytes
            rng.GetBytes(randomBytes);

            // Convert bytes to integer
            uint randomNumber = BitConverter.ToUInt32(randomBytes, 0);

            // Use modulo to get index within character set range
            int index = (int)(randomNumber % (uint)chars.Length);

            return chars[index];
        }
        /// <summary>
        /// Shuffles the string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Shuffled string</returns>
        private string ShuffleString(string input)
        {
            // Convert string to character array for shuffling
            char[] array = input.ToCharArray();

            using (var rng = RandomNumberGenerator.Create())
            {
                // Fisher-Yates shuffle algorithm
                int n = array.Length;
                while (n > 1)
                {
                    n--;

                    // Get random index using secure RNG
                    byte[] randomBytes = new byte[4];
                    rng.GetBytes(randomBytes);
                    uint randomNumber = BitConverter.ToUInt32(randomBytes, 0);
                    int k = (int)(randomNumber % (uint)(n + 1));

                    // Swap elements
                    (array[k], array[n]) = (array[n], array[k]);
                }
            }

            return new string(array);
        }
    }
}
