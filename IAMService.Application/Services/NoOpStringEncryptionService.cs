using IAMService.Application.Interfaces;
namespace IAMService.Application.Services
{
    /// <summary>
    ///     A no-op encryption service used primarily for tests or when DI isn't providing a real implementation.
    ///     It simply returns the input unchanged for both EncryptString and DecryptString.
    /// </summary>
    internal class NoOpStringEncryptionService : IStringEncryptionService
    {
        public string DecryptString(string base64CipherText)
        {
            return base64CipherText ?? string.Empty;
        }

        public string EncryptString(string plainText)
        {
            return plainText ?? string.Empty;
        }
    }
}
