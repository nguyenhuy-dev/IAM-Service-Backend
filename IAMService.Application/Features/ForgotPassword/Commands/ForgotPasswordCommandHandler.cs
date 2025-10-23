using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.ForgetPassword;
using IAMService.Domain.Entities;
using MediatR;

namespace IAMService.Application.Features.ForgotPassword.Commands
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ITokenHasher _tokenHasher;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IEmailService emailService,
            ITokenHasher tokenHasher,
            IPasswordResetTokenRepository tokenRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _tokenHasher = tokenHasher;
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return true;
            }
            if (user.IsLockedOut)
            {
                return true;
            }
            var rawToken = Guid.NewGuid().ToString("N");

            var hashedToken = _tokenHasher.Hash(rawToken);

            await _tokenRepository.DeleteExistingTokensByUserIdAsync(user.UserId);

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            await _tokenRepository.AddAsync(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var callbackUrl = $"http://localhost:5173/reset-password?userId={user.UserId}&token={Uri.EscapeDataString(rawToken)}";

            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                "Yêu cầu Đặt lại Mật khẩu",
                callbackUrl);

            return true;
        }
    }
}
