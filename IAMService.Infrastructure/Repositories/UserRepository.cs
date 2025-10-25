using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IAMService.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for User entity
    /// Handles all database operations related to users
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.IUserRepository" />
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly IAMServiceDbContext _context;
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        public UserRepository(IAMServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<User> CreateAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var createdUser = await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.Privileges)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (createdUser == null)
            {
                throw new InvalidOperationException($"Failed to retrieve created user with ID {user.UserId}");
            }
            return user;
        }
        /// <inheritdoc/>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }
        /// <inheritdoc/>
        public async Task<bool> ExistsByIdentityNumberAsync(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                throw new ArgumentException("Identity number cannot be null or empty", nameof(identityNumber));
            return await _context.Users
                .AnyAsync(u => u.IdentityNumber == identityNumber);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.Privileges)
                .AsNoTracking() // Prevent EF from tracking the entity
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            return await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.Privileges)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Updates an existing user with the provided information.
        /// </summary>
        /// <param name="user">The <see cref="T:IAMService.Domain.Entities.User" /> entity containing updated information.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task UpdateAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var trackedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (trackedUser != null)
            {
                // Update all scalar values (not navigation)
                _context.Entry(trackedUser).CurrentValues.SetValues(user);

                // Explicitly mark RoleId as modified (EF sometimes misses this)
                _context.Entry(trackedUser).Property(u => u.RoleId).IsModified = true;
            }
            else
            {
                // Fallback for detached entity
                _context.Users.Attach(user);
                _context.Entry(user).Property(u => u.RoleId).IsModified = true;
                _context.Entry(user).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the by role identifier asynchronous.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<List<User>> GetByRoleIdAsync(int roleId)
        {
            return await _context.Users
                .Where(u => u.RoleId == roleId)
                .ToListAsync();
        }

        /// <summary>
        /// Marks a collection of user entities for update. This does NOT save to the database.
        /// </summary>
        /// <param name="users">The collection of users to update.</param>
        public void UpdateRange(IEnumerable<User> users)
        {
            _context.Users.UpdateRange(users);
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="user">The user entity to delete</param>
        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }

        /// <summary>
        /// Retrieves all users with their associated roles
        /// </summary>
        /// <returns>
        /// IQueryable of users for deferred execution
        /// </returns>
        public IQueryable<User> GetUsersQueryable()
        {
            return _context.Users
                .Include(u => u.Role)
                .AsQueryable();
        }
        
    }
}
