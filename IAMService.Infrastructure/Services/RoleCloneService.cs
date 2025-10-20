using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace IAMService.Infrastructure.Services
{
    /// <summary>
    /// Service responsible for creating a custom role for a user 
    /// when an admin updates their privileges.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.IRoleCloneService" />
    public class RoleCloneService : IRoleCloneService
    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly IAMServiceDbContext _context;
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<RoleCloneService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleCloneService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public RoleCloneService(IAMServiceDbContext context, ILogger<RoleCloneService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Clones an existing user's role and assigns a customized set of privileges.
        /// </summary>
        /// <param name="user">The user whose role will be cloned.</param>
        /// <param name="privilegeIds">The identifiers of privileges to associate with the cloned role.</param>
        /// <param name="cancellationToken">The cancellation token to observe during the operation.</param>
        /// <returns>
        /// The newly created role that contains the customized set of privileges.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// RoleId {user.RoleId} not found in database.
        /// or
        /// No valid privileges found for the provided IDs.
        /// </exception>
        public async Task<Role> CloneRoleWithPrivilegesAsync(User user, List<int> privilegeIds, CancellationToken cancellationToken)
        {
            // Load the user's current role including its privileges.
            var currentRole = await _context.Roles
                .Include(r => r.Privileges)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleId == user.RoleId, cancellationToken);

            if (currentRole == null)
                throw new InvalidOperationException($"RoleId {user.RoleId} not found in database.");

            // Load the privileges corresponding to the provided privilege IDs.s
            var privileges = await _context.Privileges
                .Where(p => privilegeIds.Contains(p.PrivilegeId))
                .ToListAsync(cancellationToken);

            if (!privileges.Any())
                throw new InvalidOperationException("No valid privileges found for the provided IDs.");

            // Load all roles in the system with their privileges for comparison.
            // This is necessary to detect if an identical privilege combination already exists.
            var allRoles = await _context.Roles
                .Include(r => r.Privileges)
                .ToListAsync(cancellationToken);

            foreach (var role in allRoles)
            {
                // Sort and normalize both privilege ID lists to ensure consistent comparison.
                var existingPrivilegeIds = role.Privileges
                    .Select(p => (int)p.PrivilegeId)
                    .Distinct()
                    .OrderBy(id => id)
                    .ToList();

                var newPrivilegeIds = privilegeIds
                    .Distinct()
                    .OrderBy(id => id)
                    .ToList();

               // Debug log to help developers trace role comparison behavior.
                _logger.LogInformation(
                    "[RoleCloneService] Comparing existing RoleId={RoleId} privileges: [{Existing}] vs new: [{New}]",
                    role.RoleId,
                    string.Join(",", existingPrivilegeIds),
                    string.Join(",", newPrivilegeIds)
                );

                // If the privilege sets are identical, reuse the existing role instead of creating a duplicate.
                if (existingPrivilegeIds.SequenceEqual(newPrivilegeIds))
                {
                    _logger.LogInformation(
                        "[RoleCloneService] ✅ Found existing RoleId={RoleId} with identical privileges. Reusing existing role.",
                        role.RoleId
                    );
                    return role; // Reuse existing role
                }
            }

            // If no existing role matches, generate a new custom role name and code.
            var shortId = user.UserId.ToString()[..6]; // first 6 chars
            var privilegeKey = string.Join("_", privilegeIds.OrderBy(id => id)); // e.g. 1_3_5

            // Ensure the role name remains readable even for long privilege combinations.
            var safePrivilegeKey = privilegeKey.Length > 30
                ? privilegeKey[..30] + "..."
                : privilegeKey;

            var roleName = $"Custom_{shortId}";
            var roleCode = $"CUST_{shortId}";

            // Create a new Role entity and assign the privileges.
            var newRole = new Role(
                roleId: 0,
                roleName: roleName,
                roleCode: roleCode,
                description: $"Custom role for user {user.FullName} ({user.UserId})"
            );

            newRole.Privileges = privileges;

            // Save the new role in the database.
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync(cancellationToken);

            // Log creation details for audit purposes.
            _logger.LogInformation(
                "[RoleCloneService] Created new custom Role: {RoleName} (RoleId: {RoleId}) for UserId: {UserId}. Privileges: {PrivilegeList}",
                newRole.RoleName,
                newRole.RoleId,
                user.UserId,
                string.Join(", ", privileges.Select(p => p.PrivilegeName))
            );

            return newRole;
        }

    }
}
