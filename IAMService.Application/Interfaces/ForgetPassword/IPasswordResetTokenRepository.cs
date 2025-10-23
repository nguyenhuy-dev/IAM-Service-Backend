using IAMService.Domain.Entities;

namespace IAMService.Application.Interfaces.ForgetPassword
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken> AddAsync (PasswordResetToken token);
        Task<PasswordResetToken?> GetValidTokenByHashedTokenAsync(string hashedToken);
        void MarkAsUsed(PasswordResetToken token);
        Task DeleteExistingTokensByUserIdAsync(Guid userId);
    }
}
