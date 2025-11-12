namespace IAMService.Application.DTOs.AuthDTOs
{
    /// <summary>
    ///     Refresh token response.
    /// </summary>
    public class RefreshTokenResponse
    {
        /// <summary>
        ///     Gets or sets the access token.
        /// </summary>
        /// <value>
        ///     The access token.
        /// </value>
        public string AccessToken { get; set; } = string.Empty;
    }
}
