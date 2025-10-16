namespace IAMService.Application.DTOs
{
    /// <summary>
    /// The privilege dto class
    /// </summary>
    public class PrivilegeDto
    {
        /// <summary>
        /// Gets or sets the value of the id
        /// </summary>
        public int PrivilegeId { get; init; }
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public required string PrivilegeName { get; init; }
    }
}
