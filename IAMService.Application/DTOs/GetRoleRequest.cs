namespace IAMService.Application.DTOs
{
    /// <summary>
    ///     DTo for getting role
    /// </summary>
    public class GetRoleRequest
    {
        /// <summary>
        ///     Gets or sets the role identifier.
        /// </summary>
        /// <value>
        ///     The role identifier.
        /// </value>
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public required string RoleCode { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public ICollection<PrivilegeDto> Privileges { get; set; } = [];
    }
}
