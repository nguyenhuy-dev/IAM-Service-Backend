using IAMService.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;
namespace IAMService.Infrastructure.Services
{
    /// <summary>
    ///     Provides functionality to encrypt and decrypt strings using AES encryption.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.IStringEncryptionService" />
    public class StringEncryptionService : IStringEncryptionService
    {
        // Fixed IV (128 bits). For stronger security consider random IV per-encryption and storing alongside ciphertext.
        /// <summary>
        ///     The iv
        /// </summary>
        private static readonly byte[] IV =
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
        };
        /// <summary>
        ///     The passphrase
        /// </summary>
        private readonly string _passphrase;
        /// <summary>
        ///     Initializes a new instance of the <see cref="StringEncryptionService" /> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <exception cref="System.ArgumentException">Passphrase must be non-empty. - passphrase</exception>
        public StringEncryptionService(string passphrase)
        {
            if (string.IsNullOrWhiteSpace(passphrase))
                throw new ArgumentException("Passphrase must be non-empty.", nameof(passphrase));

            _passphrase = passphrase;
        }
        /// <summary>
        ///     Encrypts the string.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        public string EncryptString(string plainText)
        {
            if (plainText == null) return string.Empty;

            var key = DeriveKeyFromPassword(_passphrase);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = IV;

            using var output = new MemoryStream();
            using (var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                var inputBytes = Encoding.Unicode.GetBytes(plainText);
                cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                cryptoStream.FlushFinalBlock();
            }

            var encrypted = output.ToArray();
            return Convert.ToBase64String(encrypted);
        }
        /// <summary>
        ///     Decrypts the string.
        /// </summary>
        /// <param name="base64CipherText">The base64 cipher text.</param>
        /// <returns></returns>
        public string DecryptString(string base64CipherText)
        {
            if (string.IsNullOrWhiteSpace(base64CipherText)) return string.Empty;

            byte[] encryptedBytes;
            try
            {
                encryptedBytes = Convert.FromBase64String(base64CipherText);
            }
            catch
            {
                // Not base64 -> assume plaintext stored before encryption
                return base64CipherText;
            }

            // ✅ ADD TRY-CATCH FOR DECRYPT OPERATION
            try
            {
                var key = DeriveKeyFromPassword(_passphrase);
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = IV;

                using var input = new MemoryStream(encryptedBytes);
                using var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var output = new MemoryStream();
                cryptoStream.CopyTo(output);

                var plain = output.ToArray();
                return Encoding.Unicode.GetString(plain);
            }
            catch (CryptographicException)
            {
                return base64CipherText;
            }
        }

        /// <summary>
        ///     Derives the key from password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private static byte[] DeriveKeyFromPassword(string password)
        {
            var emptySalt = Array.Empty<byte>();
            var iterations = 1000;
            var desiredKeyLength = 16; // 16 bytes = 128 bits
            var hashMethod = HashAlgorithmName.SHA384;

            return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
                emptySalt,
                iterations,
                hashMethod,
                desiredKeyLength);
        }
    }
}
