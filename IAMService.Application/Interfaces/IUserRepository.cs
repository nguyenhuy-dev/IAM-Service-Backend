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
    }
}
