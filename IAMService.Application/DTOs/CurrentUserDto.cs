namespace IAMService.Application.DTOs
{
    /// <summary>
    ///     Represents the current logged-in user.
    /// </summary>
    public class CurrentUserDto
    {
        /// <summary>
        ///     Gets or sets the user identifier.
        /// </summary>
        /// <value>
        ///     The user identifier.
        /// </value>
        public Guid UserId { get; set; }
        /// <summary>
        ///     Gets or sets the name of the role.
        /// </summary>
        /// <value>
        ///     The name of the role.
        /// </value>
        public string RoleName { get; set; } = "User";
    }
}
