using IAMService.Application.Interfaces;
using IAMService.Infrastructure.Data;

namespace IAMService.Infrastructure.Repositories;

/// <summary>
/// Auth repository implementation.
/// </summary>
/// <seealso cref="IAMService.Application.Interfaces.IAuthRepository" />
public class AuthRepository(IAMServiceDbContext dbContext) : IAuthRepository
{
    /// <summary>
    /// The database context
    /// </summary>
    private readonly IAMServiceDbContext _dbContext = dbContext;

    /// <summary>
    /// Checks the valid token.
    /// </summary>
    /// <param name="tokenValue">The token value.</param>
    /// <returns></returns>
    public Task<bool> CheckValidToken(string tokenValue)
    {
        var isValidToken = _dbContext.UserTokens.Any(ut =>
            ut.Token == tokenValue &&
            !ut.IsRevoked &&
            ut.ExpirationAt > DateTime.UtcNow
        );

        return Task.FromResult(isValidToken);
    }
}
