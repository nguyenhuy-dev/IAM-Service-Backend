using IAMService.Application.Interfaces.ForgetPassword;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IAMService.Infrastructure.Repositories.ForgetPassword
{   public class PasswordResetTokenRepositoy : IPasswordResetTokenRepository
    {
        private readonly IAMServiceDbContext _dbContext;
        public PasswordResetTokenRepositoy(IAMServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PasswordResetToken> AddAsync(PasswordResetToken token)
        {
            await _dbContext.PasswordResetTokens.AddAsync(token);
            return token;
        }

        public async Task DeleteExistingTokensByUserIdAsync(Guid userId)
        {
            var tokenToDelete = await _dbContext.PasswordResetTokens
                                        .Where(x => x.UserId == userId)
                                        .ToListAsync();
            _dbContext.PasswordResetTokens.RemoveRange(tokenToDelete);
        }

        public async Task<PasswordResetToken?> GetValidTokenByHashedTokenAsync(string hashedToken)
        {
            return await _dbContext.PasswordResetTokens
                .Where(t => t.Token == hashedToken
                         && t.ExpiresAt > DateTime.UtcNow
                         && t.IsUsed == false)
                .FirstOrDefaultAsync();
        }

        public void MarkAsUsed(PasswordResetToken token)
        {
            token.IsUsed = true;
        }
    }
}
