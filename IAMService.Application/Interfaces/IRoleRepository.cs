using IAMService.Domain.Entities;
namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// The role repository interface
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Creates a new role with associated privileges.
        /// </summary>
        /// <param name="role">The role to create.</param>
        /// <param name="privilegeIds">The privilege identifiers to associate with the role.</param>
        /// <returns>The created role with its associated privileges.</returns>
        Task<Role> CreateAsync(Role role, IEnumerable<int> privilegeIds);

        /// <summary>
        /// Checks if a role with the specified code exists.
        /// </summary>
        /// <param name="roleCode">The role code.</param>
        /// <returns>True if the role exists; otherwise, false.</returns>
        Task<bool> ExistsByCodeAsync(string roleCode);

        /// <summary>
        /// Checks if a role with the specified name exists.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <returns>True if the role exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string roleName);
    }
}
