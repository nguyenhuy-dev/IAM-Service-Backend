using FluentValidation;
using IAMService.Application.Interfaces;
namespace IAMService.Application.Features.User.Commands.DeleteUser
{
    /// <summary>
    ///     Validator for DeleteUserCommand.
    /// </summary>
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        private readonly IUserRepository _userRepository;

        public DeleteUserCommandValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(u => u.UserId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("UserId is required.")
                .MustAsync(UserExistsAsync).WithMessage("User not found.");
        }

        private async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null;
        }
    }
}
