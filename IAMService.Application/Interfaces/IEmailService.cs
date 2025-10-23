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

        /// <summary>
        /// Sends an email containing the password reset link (callback URL) to the user.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="callbackUrl">The complete URL containing the token for password reset.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task representing the async operation.</returns>
        Task SendPasswordResetEmailAsync(
            string toEmail,
            string subject,
            string callbackUrl,
            CancellationToken cancellationToken = default);
    }
}