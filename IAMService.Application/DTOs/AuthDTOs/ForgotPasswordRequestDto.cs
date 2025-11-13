namespace IAMService.Application.DTOs.AuthDTOs
{
    /// <summary>
    ///     Data transfer object used to receive the user's email for the Forgot Password request.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        ///     Gets or sets the email address of the user who forgot their password.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}
