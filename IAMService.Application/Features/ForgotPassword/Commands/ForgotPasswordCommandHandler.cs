using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.ForgetPassword;
using IAMService.Domain.Entities;
using MediatR;
using System.Net.Mail;
namespace IAMService.Application.Features.ForgotPassword.Commands
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly IEmailService _emailService;
        private readonly IStringEncryptionService _stringEncryptionService;
        private readonly ITokenHasher _tokenHasher;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IEmailService emailService,
            ITokenHasher tokenHasher,
            IPasswordResetTokenRepository tokenRepository,
            IUnitOfWork unitOfWork,
            IStringEncryptionService stringEncryptionService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _tokenHasher = tokenHasher;
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
            _stringEncryptionService = stringEncryptionService;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return true; // Security: Always return true to prevent email enumeration
            }

            if (user.IsLockedOut)
            {
                return true; // Security: Don't reveal locked status
            }

            var decryptedEmail = _stringEncryptionService.DecryptString(user.Email);

            if (string.IsNullOrEmpty(decryptedEmail) || !IsValidEmail(decryptedEmail))
            {
                return true; // Security: Don't reveal invalid email status
            }

            var rawToken = Guid.NewGuid().ToString("N");
            var hashedToken = _tokenHasher.Hash(rawToken);

            await _tokenRepository.DeleteExistingTokensByUserIdAsync(user.UserId);

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            await _tokenRepository.AddAsync(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var callbackUrl = $"http://localhost:5173/reset-password?userId={user.UserId}&token={Uri.EscapeDataString(rawToken)}";

            await _emailService.SendPasswordResetEmailAsync(
                decryptedEmail,
                "Yêu cầu Đặt lại Mật khẩu",
                callbackUrl);

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
