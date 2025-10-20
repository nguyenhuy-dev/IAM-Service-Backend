namespace IAMService.Infrastructure.Settings
{
    public class EmailSettings
    {
        /// <summary>
        /// SMTP server host 
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// SMTP server port 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Email address to send from
        /// </summary>
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// Display name for sender
        /// </summary>
        public string FromName { get; set; } = string.Empty;

        /// <summary>
        /// SMTP username 
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// SMTP password or App Password
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
