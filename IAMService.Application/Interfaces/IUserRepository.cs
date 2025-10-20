using IAMService.Domain.Entities;

namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// The user repository interface.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The user identifier (GUID).</param>
        /// <returns>
        /// The <see cref="User"/> entity if found; otherwise, <c>null</c>.
        /// </returns>
        Task<User?> GetByIdAsync(Guid userId);

        /// <summary>
        /// Updates an existing user with the provided information.
        /// </summary>
        /// <param name="user">The <see cref="User"/> entity containing updated information.</param>
        /// <returns>
        /// A task that represents the asynchronous update operation.
        /// </returns>
        Task UpdateAsync(User user);

        /// <summary>Gets the by role identifier asynchronous.</summary>
        /// <param name="roleId">The role identifier.</param>
        Task<List<User>> GetByRoleIdAsync(int roleId);
        /// <summary>
        /// Marks a collection of user entities for update in the change tracker.
        /// </summary>
        /// <param name="users">The collection of users to update.</param>
        void UpdateRange(IEnumerable<User> users);
    }
}
