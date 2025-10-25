namespace IAMService.Application.Interfaces
{
    /// <summary>
    ///  Provides functionality for encrypting and decrypting string data.
    /// </summary>
    public interface IStringEncryptionService
    {
        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        string EncryptString(string plainText);
        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="base64CipherText">The base64 cipher text.</param>
        /// <returns></returns>
        string DecryptString(string base64CipherText);
    }
}
