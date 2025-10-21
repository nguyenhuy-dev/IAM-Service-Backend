using IAMService.Domain.Entities;

namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Repository interface for User entity operations
    /// Defines data access methods for user management
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The created user with generated ID</returns>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// Checks if an email address is already registered
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <returns>>True if email exists, false otherwise</returns>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Existses the by identity number asynchronous.
        /// </summary>
        /// <param name="identityNumber">The identity number.</param>
        /// <returns>True if identity number exists, false otherwise</returns>
        Task<bool> ExistsByIdentityNumberAsync(string identityNumber);

        /// Gets a user by their ID
        /// <summary>
        /// Gets a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The user identifier (GUID).</param>
        /// <returns>
        /// The <see cref="User"/> entity if found; otherwise, <c>null</c>.
        /// </returns>
        Task<User?> GetByIdAsync(Guid userId);

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetByEmailAsync(string email);

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
        /// <summary>
        /// Removes a user from the repository
        /// </summary>
        /// <param name="user">The user to delete</param>
        void Delete(User user);

        /// <summary>
        /// Gets a queryable collection of users with their roles
        /// </summary>
        /// <returns>IQueryable of users for deferred execution</returns>
        IQueryable<User> GetUsersQueryable();

    }
}
