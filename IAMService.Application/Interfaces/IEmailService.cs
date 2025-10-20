namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Interface for email notification services
    /// Provides methods to send various types of emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a welcome email with auto-generated password to a new patient user
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="fullName">Full name of the user</param>
        /// <param name="generatedPassword">Auto-generated password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task SendNewPatientAccountEmailAsync(
            string toEmail,
            string fullName,
            string generatedPassword,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a welcome email to a new employee user
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="fullName">Full name of the user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task SendNewEmployeeAccountEmailAsync(
            string toEmail,
            string fullName,
            CancellationToken cancellationToken = default);
    }
}