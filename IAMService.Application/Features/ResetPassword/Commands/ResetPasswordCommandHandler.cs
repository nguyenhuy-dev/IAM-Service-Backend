using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.ForgetPassword;
using MediatR;

namespace IAMService.Application.Features.ResetPassword.Commands
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenHasher _tokenHasher;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(IUserRepository userRepository, ITokenHasher tokenHasher, IPasswordHasher passwordHasher, IPasswordResetTokenRepository tokenRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenHasher = tokenHasher;
            _passwordHasher = passwordHasher;
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            
            var hashedToken = _tokenHasher.Hash(request.Token);
            var tokenEntity = await _tokenRepository.GetValidTokenByHashedTokenAsync(hashedToken);
            if (tokenEntity == null || tokenEntity.UserId != request.UserId)
            {
                return false;
            }
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null || user.IsLockedOut)
            {
                return false;
            }

            // Băm mật khẩu mới và cập nhật
            user.HashedPassword = _passwordHasher.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            _tokenRepository.MarkAsUsed(tokenEntity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Tùy chọn: Gửi email thông báo mật khẩu đã thay đổi thành công

            return true;
        }
    }   
}
