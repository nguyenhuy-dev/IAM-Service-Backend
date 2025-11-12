using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace IAMService.Infrastructure.Repositories
{
    /// <summary>
    ///     Auth repository implementation.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.AuthenticationServices.IAuthRepository" />
    /// <seealso cref="Application.Interfaces.AuthenticationServices.IAuthRepository" />
    public class AuthRepository(
        IAMServiceDbContext dbContext,
        IPasswordHasher passwordHasher,
        IStringEncryptionService stringEncryptionService,
        IConfiguration configuration) : IAuthRepository
    {

        /// <summary>
        ///     The configuration
        /// </summary>
        private readonly IConfiguration _configuration = configuration;
        /// <summary>
        ///     The database context
        /// </summary>
        private readonly IAMServiceDbContext _dbContext = dbContext;

        /// <summary>
        ///     The password hasher
        /// </summary>
        private readonly IPasswordHasher _passwordHasher = passwordHasher;

        /// <summary>
        ///     The string encryption service
        /// </summary>
        private readonly IStringEncryptionService _stringEncryptionService = stringEncryptionService;

        /// <summary>
        ///     Adds the JWT token.
        /// </summary>
        /// <param name="jwtToken">The JWT token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task AddJwtToken(JwtToken jwtToken, CancellationToken cancellationToken)
        {
            await _dbContext.JwtTokens.AddAsync(jwtToken, cancellationToken);
        }

        /// <summary>
        ///     Checks the valid token.
        /// </summary>
        /// <param name="tokenValue">The token value.</param>
        /// <returns></returns>
        public Task<bool> CheckValidToken(string tokenValue)
        {
            var isValidToken = _dbContext.JwtTokens
                .Any(t =>
                    t.AccessToken == tokenValue &&
                    !t.IsRevoked &&
                    t.ReTokenExpireAt > DateTime.UtcNow
                );

            return Task.FromResult(isValidToken);
        }

        /// <summary>
        ///     Deletes all JWT tokens.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<int> DeleteAllJwtTokens(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.JwtTokens
                .Where(j => j.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <summary>
        ///     Logins the specified email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.UnauthorizedAccessException">
        ///     Account non-available.
        ///     or
        ///     Password incorrect.
        /// </exception>
        public async Task<User> Login(string email, string password, CancellationToken cancellationToken)
        {
            var encryptedEmail = _stringEncryptionService.EncryptString(email);
            var user = await _dbContext.Users
                           .AsNoTracking()
                           .Include(u => u.Role)
                           .FirstOrDefaultAsync(u => u.Email == encryptedEmail &&
                                                     u.IsActive &&
                                                     (!u.LockoutEnd.HasValue || u.LockoutEnd < DateTimeOffset.UtcNow), cancellationToken)
                    ?? throw new UnauthorizedAccessException("Account non-available.");

            var hashedPassword = user.HashedPassword;
            if (!_passwordHasher.VerifyPassword(hashedPassword, password))
            {
                await LockoutAccount(user, cancellationToken);
                throw new UnauthorizedAccessException("Password incorrect.");
            }

            if (user.FailedLoginAttempts != 0 || user.LockoutEnd is not null)
                await ResetAttemptsOrLockoutEnd(user, cancellationToken);

            // Decrypt infor
            user.Email = email;
            user.FullName = _stringEncryptionService.DecryptString(user.FullName);

            return user;
        }

        /// <summary>
        ///     Gets the user with old access token.
        /// </summary>
        /// <param name="oldAccessToken">The old access token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.UnauthorizedAccessException">Refresh token unvalid!</exception>
        public async Task<User> GetUserWithOldAccessToken(string oldAccessToken, CancellationToken cancellationToken)
        {
            var jwtToken = await _dbContext.JwtTokens
                               .Include(jt => jt.User)
                               .ThenInclude(u => u.Role)
                               .FirstOrDefaultAsync(jt => jt.AccessToken == oldAccessToken &&
                                                          !jt.IsRevoked &&
                                                          jt.ReTokenExpireAt > DateTime.UtcNow, cancellationToken)
                        ?? throw new UnauthorizedAccessException("Refresh token unvalid!");

            var user = jwtToken.User;

            return user;
        }

        /// <summary>
        ///     Resets the attempts or lockout end.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task ResetAttemptsOrLockoutEnd(User user, CancellationToken cancellationToken)
        {
            if (user.FailedLoginAttempts != 0)
            {
                user.ResetAttempts();
                await _dbContext.Users
                    .Where(u => u.UserId == user.UserId)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.FailedLoginAttempts, user.FailedLoginAttempts), cancellationToken);
            }

            if (user.LockoutEnd is not null)
            {
                user.UnlockAccount();
                await _dbContext.Users
                    .Where(u => u.UserId == user.UserId)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.LockoutEnd, user.LockoutEnd), cancellationToken);
            }
        }

        /// <summary>
        ///     Lockouts the account.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.InvalidOperationException">
        ///     Can't read environment variables '{nameof(maxFailedLoginAttempts)}'
        ///     or '{nameof(lockoutEndDays)}'.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">Account is locked out.</exception>
        private async Task LockoutAccount(User user, CancellationToken cancellationToken)
        {
            var sectionLockoutPolicy = _configuration.GetSection("LockoutPolicy");
            if (!int.TryParse(sectionLockoutPolicy["MaxFailedLoginAttempts"], out var maxFailedLoginAttempts) ||
                !double.TryParse(sectionLockoutPolicy["LockoutEndDays"], out var lockoutEndDays))
                throw new InvalidOperationException($"Can't read environment variables '{nameof(maxFailedLoginAttempts)}' or '{nameof(lockoutEndDays)}'.");

            if (user.FailedLoginAttempts == maxFailedLoginAttempts)
            {
                user.LockAccount(DateTimeOffset.UtcNow.AddDays(lockoutEndDays));
                user.ResetAttempts();
                await _dbContext.Users
                    .Where(u => u.UserId == user.UserId)
                    .ExecuteUpdateAsync(s => s
                            .SetProperty(u => u.LockoutEnd, user.LockoutEnd)
                            .SetProperty(u => u.FailedLoginAttempts, user.FailedLoginAttempts), cancellationToken
                    );
                throw new UnauthorizedAccessException("Account is locked out.");
            }

            user.IncrementFailedAttempts();
            await _dbContext.Users
                .Where(u => u.UserId == user.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.FailedLoginAttempts, user.FailedLoginAttempts), cancellationToken);
        }
    }
}
