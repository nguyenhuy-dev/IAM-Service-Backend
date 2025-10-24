using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace IAMService.Infrastructure.Repositories;

/// <summary>
/// Auth repository implementation.
/// </summary>
/// <seealso cref="IAMService.Application.Interfaces.AuthenticationServices.IAuthRepository" />
/// <seealso cref="Application.Interfaces.AuthenticationServices.IAuthRepository" />
public class AuthRepository(IAMServiceDbContext dbContext, IPasswordHasher passwordHasher) : IAuthRepository
{
    /// <summary>
    /// The database context
    /// </summary>
    private readonly IAMServiceDbContext _dbContext = dbContext;

    /// <summary>
    /// The password hasher
    /// </summary>
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Adds the JWT token.
    /// </summary>
    /// <param name="jwtToken">The JWT token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AddJwtToken(JwtToken jwtToken, CancellationToken cancellationToken)
    {
        await _dbContext.JwtTokens.AddAsync(jwtToken, cancellationToken);
    }

    /// <summary>
    /// Checks the valid token.
    /// </summary>
    /// <param name="tokenValue">The token value.</param>
    /// <returns></returns>
    public Task<bool> CheckValidToken(string tokenValue)
    {
        var isValidToken = _dbContext.JwtTokens
            .Where(t => t.AccessToken == tokenValue)
            .OrderByDescending(t => t.CreateAt)
            .Any(t =>
                !t.IsRevoked &&
                t.ReTokenExpireAt > DateTime.UtcNow
            );

        return Task.FromResult(isValidToken);
    }

    /// <summary>
    /// Deletes all JWT tokens.
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
    /// Logins the specified email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="System.UnauthorizedAccessException">
    /// Account non-available.
    /// or
    /// Password incorrect.
    /// </exception>
    public async Task<User> Login(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken) 
            ?? throw new UnauthorizedAccessException("Account non-available.");

        var hashedPassword = user.HashedPassword;
        if (!_passwordHasher.VerifyPassword(hashedPassword, password))
            throw new UnauthorizedAccessException("Password incorrect.");

        return user;
    }

    /// <summary>
    /// Gets the user with old access token.
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
}
