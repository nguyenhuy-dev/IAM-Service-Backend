using IAMService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace IAMService.Infrastructure.Services
{
    /// <summary>
    ///     Simple audit logging service
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly ILogger<AuditLogService> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLogService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public AuditLogService(ILogger<AuditLogService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Logs user creation event
        /// </summary>
        /// <param name="userId">ID of the created user</param>
        /// <param name="email">Email of the created user</param>
        /// <param name="userType">Type of user (Employee/Patient)</param>
        /// <param name="createdBy">ID or name of the person who created the user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task LogUserCreationAsync(
            Guid userId,
            string email,
            string userType,
            string createdBy,
            CancellationToken cancellationToken = default)
        {
            var auditLog = new
            {
                Event = "UserCreation",
                UserId = userId,
                Email = email,
                UserType = userType,
                CreatedBy = createdBy,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Log as JSON format
            _logger.LogInformation(
                "🔒 AUDIT: {AuditLog}",
                JsonSerializer.Serialize(auditLog, new JsonSerializerOptions
                {
                    WriteIndented = false
                }));

            await Task.CompletedTask;
        }
    }
}
