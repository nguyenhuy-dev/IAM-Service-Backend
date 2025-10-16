namespace IAMService.Application.DTOs
{
    /// <summary>
    /// The create role request class
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// Gets or sets the value of the role name
        /// </summary>
        public string RoleName { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the value of the role code
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the value of the description
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
