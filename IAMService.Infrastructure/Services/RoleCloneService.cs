using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace IAMService.Infrastructure.Services
{
    /// <summary>
    ///     Service responsible for creating a custom role for a user
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.IRoleCloneService" />
    public class RoleCloneService : IRoleCloneService
    {
        /// <summary>
        ///     The context
        /// </summary>
        private readonly IAMServiceDbContext _context;
        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger<RoleCloneService> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleCloneService" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public RoleCloneService(IAMServiceDbContext context, ILogger<RoleCloneService> logger)
        {
            _context = context;
            _logger = logger;
        }
        /// <summary>
        ///     Clones an existing user's role and assigns a customized set of privileges.
        /// </summary>
        /// <param name="user">The user whose role will be cloned.</param>
        /// <param name="privilegeIds">The identifiers of privileges to associate with the cloned role.</param>
        /// <param name="cancellationToken">The cancellation token to observe during the operation.</param>
        /// <returns>
        ///     The newly created role that contains the customized set of privileges.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Privilege list cannot be empty.
        ///     or
        ///     No valid privileges found for provided IDs.
        /// </exception>
        public async Task<Role> CloneRoleWithPrivilegesAsync(User user, List<int> privilegeIds, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (privilegeIds == null || privilegeIds.Count == 0)
                throw new InvalidOperationException("Privilege list cannot be empty.");

            var newPrivilegeIds = privilegeIds.Distinct().OrderBy(id => id).ToList();

            //  Load privileges safely
            var privileges = await _context.Privileges
                .Where(p => newPrivilegeIds.Contains(p.PrivilegeId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!privileges.Any())
                throw new InvalidOperationException("No valid privileges found for provided IDs.");

            //  Check if identical role already exists (exact privilege match)
            var existingRole = await _context.Roles
                .Include(r => r.Privileges)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var role in existingRole)
            {
                var rolePrivilegeIds = role.Privileges.Select(p => p.PrivilegeId).OrderBy(x => x).ToList();
                if (rolePrivilegeIds.SequenceEqual(newPrivilegeIds))
                {
                    _logger.LogInformation("♻️ Reusing existing role {RoleId} ({RoleName})", role.RoleId, role.RoleName);
                    return role;
                }
            }

            //  Create new role safely (unique version)
            var shortId = user.UserId.ToString()[..6];
            var baseName = $"Custom_{shortId}";
            var roleName = baseName;
            var version = 1;
            while (await _context.Roles.AnyAsync(r => r.RoleName == roleName, cancellationToken))
                roleName = $"{baseName}_v{++version}";

            var newRole = new Role(0, roleName, $"CUST_{shortId}", $"Custom role for {user.FullName}");

            //  Properly attach privileges (no duplication)
            newRole.Privileges = new List<Privilege>();
            foreach (var privilege in privileges)
            {
                var trackedPrivilege = _context.Privileges.Local
                                           .FirstOrDefault(p => p.PrivilegeId == privilege.PrivilegeId)
                                    ?? _context.Privileges.Attach(privilege).Entity;

                newRole.Privileges.Add(trackedPrivilege);
            }

            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Created new role {RoleName} (RoleId={RoleId}) for user {UserId} with privileges: {Privileges}",
                newRole.RoleName, newRole.RoleId, user.UserId, string.Join(", ", privileges.Select(p => p.PrivilegeName)));

            return newRole;
        }
    }
}
