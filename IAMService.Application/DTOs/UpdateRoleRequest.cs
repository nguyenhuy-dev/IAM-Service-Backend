namespace IAMService.Application.DTOs
{
    /// <summary>
    ///     The update role request
    /// </summary>
    public record UpdateRoleRequest(
        string RoleName,
        string RoleCode,
        string Description,
        IEnumerable<int> PrivilegeIds
    );
}
