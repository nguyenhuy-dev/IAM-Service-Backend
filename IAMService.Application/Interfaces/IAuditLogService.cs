namespace IAMService.Application.Interfaces
{
    /// <summary>
    ///     Interface for audit logging service
    ///     Records important system events and user actions for compliance and security
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        ///     Logs user creation event
        /// </summary>
        /// <param name="userId">ID of the created user</param>
        /// <param name="email">Email of the created user</param>
        /// <param name="userType">Type of user (Employee/Patient)</param>
        /// <param name="createdBy">ID or name of the person who created the user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task LogUserCreationAsync(
            Guid userId,
            string email,
            string userType,
            string createdBy,
            CancellationToken cancellationToken = default);
    }
}
