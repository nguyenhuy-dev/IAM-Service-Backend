using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IAMService.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing user data.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.IUserRepository" />
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly IAMServiceDbContext _context;
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public UserRepository(IAMServiceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The user identifier (GUID).</param>
        /// <returns>
        /// The <see cref="T:IAMService.Domain.Entities.User" /> entity if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<User?> GetByIdAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.Privileges) 
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user != null)
            {
                user.Age = CalculateAge(user.DateOfBirth);
            }

            return user;
        }
        private int CalculateAge(DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// Updates an existing user with the provided information.
        /// </summary>
        /// <param name="user">The <see cref="T:IAMService.Domain.Entities.User" /> entity containing updated information.</param>
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>Gets the by role identifier asynchronous.</summary>
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
    }
}
