using IAMService.Domain.Entities;
namespace IAMService.Application.Interfaces
{

    /// <summary>
    /// The privilege repository interface
    /// </summary>
    public interface IPrivilegeRepository
    {

        /// <summary>
        /// Creates a new privilege.
        /// </summary>
        /// <param name="privilege">The privilege to create.</param>
        /// <returns>The created privilege.</returns>
        Task<Privilege> CreateAsync(Privilege privilege);

        /// <summary>
        /// Gets a privilege by identifier.
        /// </summary>
        /// <param name="privilegeId">The privilege identifier.</param>
        /// <returns>The privilege if found; otherwise, null.</returns>
        Task<Privilege?> GetByIdAsync(int privilegeId);

        /// <summary>
        /// Gets a privilege by name.
        /// </summary>
        /// <param name="privilegeName">The privilege name.</param>
        /// <returns>The privilege if found; otherwise, null.</returns>
        Task<Privilege?> GetByNameAsync(string privilegeName);

        /// <summary>
        /// Gets multiple privileges by their identifiers.
        /// </summary>
        /// <param name="privilegeIds">The privilege identifiers.</param>
        /// <returns>A collection of privileges matching the provided identifiers.</returns>
        Task<IEnumerable<Privilege>> GetByIdsAsync(IEnumerable<int> privilegeIds);

        /// <summary>
        /// Gets all privileges.
        /// </summary>
        /// <returns>A collection of all privileges.</returns>
        Task<IEnumerable<Privilege>> GetAllAsync();

        /// <summary>
        /// Updates an existing privilege.
        /// </summary>
        /// <param name="privilege">The privilege to update.</param>
        /// <returns>The updated privilege.</returns>
        Task<Privilege> UpdateAsync(Privilege privilege);

        /// <summary>
        /// Deletes a privilege by identifier.
        /// </summary>
        /// <param name="privilegeId">The privilege identifier.</param>
        /// <returns>True if the privilege was deleted; otherwise, false.</returns>
        Task<bool> DeleteAsync(int privilegeId);

        /// <summary>
        /// Checks if a privilege with the specified name exists.
        /// </summary>
        /// <param name="privilegeName">The privilege name.</param>
        /// <returns>True if the privilege exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string privilegeName);

        /// <summary>
        /// Checks if all specified privilege identifiers exist.
        /// </summary>
        /// <param name="privilegeIds">The privilege identifiers to check.</param>
        /// <returns>True if all privileges exist; otherwise, false.</returns>
        Task<bool> AllExistAsync(IEnumerable<int> privilegeIds);
    }
}
