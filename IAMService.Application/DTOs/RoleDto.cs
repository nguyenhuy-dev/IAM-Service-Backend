namespace IAMService.Application.DTOs
{
    /// <summary>
    ///     The role dto class
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        ///     Gets or sets the value of the id
        /// </summary>
        public int RoleId { get; init; }
        /// <summary>
        ///     Gets or sets the value of the name
        /// </summary>
        public required string RoleName { get; init; }
        /// <summary>
        ///     Gets or sets the value of the code
        /// </summary>
        public required string RoleCode { get; init; }
        /// <summary>
        ///     Gets or sets the value of the description
        /// </summary>
        public required string Description { get; init; }
        /// <summary>
        ///     Gets or sets the value of the privileges
        /// </summary>
        public required List<PrivilegeDto> Privileges { get; init; }
    }
}
