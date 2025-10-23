using IAMService.Application.DTOs.Auth.Login;
using IAMService.Application.Exceptions;
using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;

namespace IAMService.Infrastructure.Services
{
    /// <summary>
    /// Provides core authentication and user management services, including login, logout, and password hashing.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtConfiguration _jwtConfiguration;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtConfiguration jwtConfiguration,
            IRefreshTokenService refreshTokenService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtConfiguration = jwtConfiguration;
            _refreshTokenService = refreshTokenService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Authenticates a user with the provided credentials, manages login attempts, and generates tokens upon success.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>A <see cref="TokenResponse"/> containing the access and refresh tokens, and user details.</returns>
        /// <exception cref="Exception">Throws if the user is not found, account is locked, or password is invalid.</exception>
        public async Task<TokenResponse> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new NotFoundException(nameof(User), email);
            }
            if (user.IsLockedOut && user.LockoutEnd.HasValue && user.LockoutEnd.Value < DateTimeOffset.UtcNow)
            {
                user.UnlockAccount();
                throw new NotFoundException("User not found!");
            }

            if (user.IsLockedOut)
            {
                throw new ForbiddenAccessException("User account is locked. Please try again later.");
            }

            var passwordMatches = _passwordHasher.VerifyPassword(user.HashedPassword, password);

            if (!passwordMatches)
            {
                user.IncrementFailedAttempts();
                var maxAttempts = _jwtConfiguration.MaxFailedAccessAttempts;

                if (user.FailedLoginAttempts >= maxAttempts)
                {
                    var lockoutMinutes = _jwtConfiguration.DefaultLockoutMinutes;
                    var lockoutUntil = DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes);
                    user.LockAccount(lockoutUntil);

                    await _userRepository.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                    throw new ForbiddenAccessException($"Invalid Password. Account locked for {lockoutMinutes} minutes.");
                }

                // Save failed attempt count
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                throw new InvalidCredentialsException("Invalid Email or Password");
            }

            // Successful Login: Reset attempts and generate tokens
            if (user.FailedLoginAttempts > 0)
            {
                user.ResetAttempts();
                await _userRepository.UpdateAsync(user);

            }
            if (user.Role == null)
            {
                throw new InvalidOperationException($"User ID {user.UserId} is missing Role data required for token creation.");
            }
            var roleEntity = user.Role;
            if (roleEntity == null || string.IsNullOrEmpty(roleEntity.RoleCode))
            {
                throw new InvalidOperationException($"User ID {user.UserId} does not have a valid RoleCode assigned.");
            }
            return await _refreshTokenService.CreateTokensAndSaveChanges(user.UserId, roleEntity.RoleCode);
        }

        /// <summary>
        /// Initiates the forgot password process for a user.
        /// </summary>
        /// <param name="username">The username or email address associated with the account.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ForgotPassword(string username)
        {
            // TODO: Implement password reset logic (e.g., sending an email with a token/link).
            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks the active status and validity of a refresh token string.
        /// </summary>
        /// <param name="token">The token string to check.</param>
        /// <returns>A task that returns true if the token is active, valid, and not revoked; otherwise, false.</returns>
        public Task<bool> IsActive(string token)
        {
            // The logic for checking token status should likely reside in IRefreshTokenService
            return Task.FromResult(false); // Placeholder
        }

        /// <summary>
        /// Invalidates and revokes a refresh token, effectively logging the user out.
        /// </summary>
        /// <param name="refreshToken">The refresh token string to be revoked.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogoutAsync(string refreshToken)
        {
            await _refreshTokenService.RevokeTokenByStringAsync(refreshToken);
        }
    }
}