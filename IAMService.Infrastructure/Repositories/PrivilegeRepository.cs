using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace IAMService.Infrastructure.Repositories
{
    /// <summary>
    ///     Repository implementation for Privilege entity operations.
    /// </summary>
    public class PrivilegeRepository(IAMServiceDbContext context) : IPrivilegeRepository
    {
        /// <summary>
        ///     The context
        /// </summary>
        private readonly IAMServiceDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <inheritdoc />
        public async Task<Privilege> CreateAsync(Privilege privilege)
        {
            ArgumentNullException.ThrowIfNull(privilege);

            await _context.Privileges.AddAsync(privilege);
            await _context.SaveChangesAsync();

            return privilege;
        }

        /// <inheritdoc />
        public async Task<Privilege?> GetByIdAsync(int privilegeId)
        {
            return await _context.Privileges
                .FirstOrDefaultAsync(p => p.PrivilegeId == privilegeId);
        }

        /// <inheritdoc />
        public async Task<Privilege?> GetByNameAsync(string privilegeName)
        {
            if (string.IsNullOrWhiteSpace(privilegeName))
                throw new ArgumentException("Privilege name cannot be null or empty", nameof(privilegeName));

            return await _context.Privileges
                .FirstOrDefaultAsync(p => p.PrivilegeName == privilegeName);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Privilege>> GetByIdsAsync(IEnumerable<int> privilegeIds)
        {
            ArgumentNullException.ThrowIfNull(privilegeIds);

            var idsList = privilegeIds.ToList();

            if (idsList.Count == 0)
                return [];

            return await _context.Privileges
                .Where(p => idsList.Contains(p.PrivilegeId))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Privilege>> GetAllAsync()
        {
            return await _context.Privileges
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Privilege> UpdateAsync(Privilege privilege)
        {
            ArgumentNullException.ThrowIfNull(privilege);

            _context.Privileges.Update(privilege);
            await _context.SaveChangesAsync();

            return privilege;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int privilegeId)
        {
            var privilege = await _context.Privileges
                .FirstOrDefaultAsync(p => p.PrivilegeId == privilegeId);

            if (privilege == null)
                return false;

            _context.Privileges.Remove(privilege);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(string privilegeName)
        {
            if (string.IsNullOrWhiteSpace(privilegeName))
                throw new ArgumentException("Privilege name cannot be null or empty", nameof(privilegeName));

            var query = _context.Privileges.Where(p => p.PrivilegeName == privilegeName);

            return await query.AnyAsync();
        }

        /// <inheritdoc />
        public async Task<bool> AllExistAsync(IEnumerable<int> privilegeIds)
        {
            ArgumentNullException.ThrowIfNull(privilegeIds);

            var idsList = privilegeIds.ToList();

            if (idsList.Count == 0)
                return true;

            var count = await _context.Privileges
                .CountAsync(p => idsList.Contains(p.PrivilegeId));

            return count == idsList.Count;
        }
    }
}
