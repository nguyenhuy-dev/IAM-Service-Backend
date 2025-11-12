namespace IAMService.Application.DTOs.AuthDTOs
{
    /// <summary>
    ///     Data transfer object used to reset a user's password.
    /// </summary>
    public class ResetPasswordRequestDto
    {
        /// <summary>
        ///     Gets or sets the user identifier.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        ///     Gets or sets the reset token.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the new password.
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the confirm password.
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
