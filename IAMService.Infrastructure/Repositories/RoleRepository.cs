using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IAMService.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Role entity operations.
    /// </summary>
    public class RoleRepository(IAMServiceDbContext context) : IRoleRepository
    {
        private readonly IAMServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <inheritdoc/>
        public async Task<Role> CreateAsync(Role role, IEnumerable<int> privilegeIds)
        {
            ArgumentNullException.ThrowIfNull(role);

            var privilegeIdsList = privilegeIds.ToList();

            if (privilegeIdsList.Count != 0)
            {
                var privileges = await _context.Privileges
                    .Where(p => privilegeIdsList.Contains(p.PrivilegeId))
                    .ToListAsync();

                if (privileges.Count != privilegeIdsList.Count)
                    throw new FluentValidation.ValidationException("One or more privilege IDs are invalid.");

                foreach (var privilege in privileges)
                {
                    role.Privileges.Add(privilege);
                }
            }

            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();

            return role;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsByCodeAsync(string roleCode)
        {
            if (string.IsNullOrWhiteSpace(roleCode))
                throw new ArgumentException("Role code cannot be null or empty", nameof(roleCode));

            return await _context.Roles
                .AnyAsync(r => r.RoleCode == roleCode);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

            var query = _context.Roles.Where(r => r.RoleName == roleName);


            return await query.AnyAsync();
        }

        /// <summary>
        /// Gets the role asynchronous.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Role> GetRoleWithPrivileges()
        {
            return _context.Roles
                .Include(r => r.Privileges);
        }
    }
}
