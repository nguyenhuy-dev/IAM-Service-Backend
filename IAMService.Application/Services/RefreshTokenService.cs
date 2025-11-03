using IAMService.Application.DTOs;
using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using System.Security.Cryptography; // Added using directive for full completeness

namespace IAMService.Application.Services
{
    /// <summary>
    /// Implements the business logic for creating, managing, rotating, and revoking Refresh Tokens.
    /// This service orchestrates operations across token generation, hashing, persistence (repository), and user data.
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ITokenHasher _tokenHasher;
        private readonly IJwtConfiguration _jwtConfiguration;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenService"/> class.
        /// </summary>
        /// <param name="refreshTokenRepository">The repository for persistence of refresh tokens.</param>
        /// <param name="tokenGenerator">The service responsible for generating JWTs and opaque token strings.</param>
        /// <param name="tokenHasher">The service for hashing raw token strings.</param>
        /// <param name="jwtConfiguration">The application configuration abstraction for JWT settings.</param>
        /// <param name="userRepository">The repository for accessing user data.</param>
        /// <param name="unitOfWork">The unit of work for managing database transactions.</param>
        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, ITokenGenerator tokenGenerator, ITokenHasher tokenHasher, IJwtConfiguration jwtConfiguration, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _tokenGenerator = tokenGenerator;
            _tokenHasher = tokenHasher;
            _jwtConfiguration = jwtConfiguration;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Generates an Access Token and a Refresh Token, saves the refresh token hash to the database, and returns the token pair.
        /// </summary>
        /// <param name="userId">The ID of the user (Guid).</param>
        /// <param name="role">The collection of roles for the user.</param>
        /// <returns>A <see cref="TokenResponse"/> containing the access token, refresh token, expiry time, and user details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user ID is not found in the repository.</exception>
        public async Task<TokenResponse> CreateTokensAndSaveChanges(Guid userId,string roleCode)
        {
            var userEntity = await _userRepository.GetByIdAsync(userId);
            if (userEntity == null) { throw new KeyNotFoundException($"User with ID {userId} not found."); }
            var roleEntity = userEntity.Role;
            if (roleEntity == null)
            {
                throw new InvalidOperationException($"Role entity is missing for user ID {userId}. Cannot issue token.");
            }

            var (accessToken, accessExpiresIn) = _tokenGenerator.GenerateAccessToken(userId, roleCode);
            var refreshTokenString = _tokenGenerator.GenerateRefreshTokenString();
            var refreshLifeTimeDays = _jwtConfiguration.RefreshTokenLifetimeDays;
            var expiresAt = DateTime.UtcNow.AddDays(refreshLifeTimeDays);
            var refreshTokenHash = _tokenHasher.Hash(refreshTokenString);


            var newEntity = new RefreshToken(
            userId: userId,
            tokenHash: refreshTokenHash,
            expiresAt: expiresAt
            );

            await _refreshTokenRepository.AddAsync(newEntity);

            var roleDTO = new RoleDto
            {
                RoleId = roleEntity.RoleId,
                RoleName = roleEntity.RoleName,
                RoleCode = roleEntity.RoleCode, // Đã sử dụng RoleCode
                Description = roleEntity.Description,
                Privileges = new List<PrivilegeDto>(), // Lưu ý: Cần load Privileges nếu cần
            };

            var userDTO = new UserDto
            {
                UserId = userEntity.UserId,
                FullName = userEntity.FullName,
                PhoneNumber = userEntity.PhoneNumber,
                Email = userEntity.Email,
                Gender = userEntity.Gender ? "Male" : "Female",
                IdentityNumber = userEntity.IdentityNumber,
                Address = userEntity.Address,
                Role = roleDTO,
                Age = userEntity.Age,
                DateOfBirth = userEntity.DateOfBirth,
                NeedsVerification = userEntity.NeedsVerification,
                IsPatient = userEntity.IsPatient,
                GeneratedPassword = null
            };

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            return new TokenResponse(accessToken, accessExpiresIn, refreshTokenString, userDTO);
        }

        /// <summary>
        /// Rotates the refresh token: generates a new token pair, revokes the old refresh token, and links the new one to the old one.
        /// </summary>
        /// <param name="oldRefreshTokenString">The raw string of the old refresh token provided by the client.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the new <see cref="TokenResponse"/>.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the token is invalid, expired, or if token reuse is detected.</exception>
        public async Task<TokenResponse> RotateAndRevokeAsync(string oldRefreshTokenString)
        {
            // 1. Hash the old token string to find the entity in the database
            var oldTokenHash = _tokenHasher.Hash(oldRefreshTokenString);
            var oldTokenEntity = await _refreshTokenRepository.GetByTokenHashAsync(oldTokenHash);

            if (oldTokenEntity == null)
            {
                throw new UnauthorizedAccessException("Refresh token invalid or expired. Please re-login!");
            }
            if (!oldTokenEntity.IsActive) // Giả định IsActive check cả RevokedAt và ExpiredAt
            {
                throw new UnauthorizedAccessException("Refresh token has expired or is invalid. Please re-login!");
            }
            // 3. Anti-Reuse Logic (Core security rule)
            if (oldTokenEntity.ReplacedByTokenId.HasValue || oldTokenEntity.RevokedAt.HasValue)
            {
                // Revoke all tokens for the user due to suspected token theft/reuse
                await RevokeAllTokensForUserAsync(oldTokenEntity.UserId);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
                throw new UnauthorizedAccessException("Security warning: Token reuse detected. All sessions have been revoked.");
            }

            var userEntity = oldTokenEntity.User;
            var roleEntity = oldTokenEntity.User?.Role;
            if (roleEntity == null || userEntity == null)
            {
                throw new InvalidOperationException($"User or Role entity is missing for user ID {oldTokenEntity.UserId}. Cannot rotate token.");
            }
            var roleCode = roleEntity.RoleCode;
            // 5. Rotation: Create new token and save (reusing CreateTokensAndSaveChanges logic)
            var (accessToken, accessExpiresIn) = _tokenGenerator.GenerateAccessToken(oldTokenEntity.UserId, roleCode);

            // 6. Revocation & Linking
            var newRefreshTokenString = _tokenGenerator.GenerateRefreshTokenString();
            var refreshLifeTimeDays = _jwtConfiguration.RefreshTokenLifetimeDays;
            var expiresAt = DateTime.UtcNow.AddDays(refreshLifeTimeDays);
            var newRefreshTokenHash = _tokenHasher.Hash(newRefreshTokenString);

            var newRefreshTokenEntity = new RefreshToken(
                userId: oldTokenEntity.UserId,
                tokenHash: newRefreshTokenHash,
                expiresAt: expiresAt
            );
            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            var newTokenId = newRefreshTokenEntity.Id;
            oldTokenEntity.Revoke(newTokenId);
            await _refreshTokenRepository.UpdateAsync(oldTokenEntity);

            var roleDTO = new RoleDto
            {
                RoleId = roleEntity.RoleId,
                RoleName = roleEntity.RoleName,
                RoleCode = roleEntity.RoleCode,
                Description = roleEntity.Description,
                Privileges = new List<PrivilegeDto>(),
            };

            var userDTO = new UserDto
            {
                UserId = userEntity.UserId,
                FullName = userEntity.FullName,
                PhoneNumber = userEntity.PhoneNumber,
                Email = userEntity.Email,
                Gender = userEntity.Gender ? "Male" : "Female",
                IdentityNumber = userEntity.IdentityNumber,
                Address = userEntity.Address,
                Role = roleDTO,
                Age = userEntity.Age,
                DateOfBirth = userEntity.DateOfBirth,
                NeedsVerification = userEntity.NeedsVerification,
                IsPatient = userEntity.IsPatient,
                GeneratedPassword = null
            };
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            // 7. Return the new token pair to the client
            return new TokenResponse(accessToken, accessExpiresIn, newRefreshTokenString, userDTO);
        }

        /// <summary>
        /// Revokes all active refresh tokens associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user (Guid).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning <c>true</c> if the operation was successful.</returns>
        public async Task<bool> RevokeAllTokensForUserAsync(Guid userId)
        {
            var activeTokens = await _refreshTokenRepository.GetActiveTokenByUserIdAsync(userId);
            if (activeTokens == null || activeTokens.Count == 0)
            {
                return true;
            }
            foreach (var token in activeTokens)
            {
                token.Revoke();
                await _refreshTokenRepository.UpdateAsync(token);
            }

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            return true;
        }

        /// <summary>
        /// Permanently removes all expired refresh tokens from the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the count of tokens deleted.</returns>
        public async Task<int> CleanupExpiredTokenAsync()
        {
            return await _refreshTokenRepository.CleanupExpiredTokenAsync();
        }

        /// <summary>
        /// Revokes a specific refresh token by its unique ID.
        /// </summary>
        /// <param name="tokenId">The ID of the token (Guid).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning <c>true</c> if the token was found and successfully revoked.</returns>
        public async Task<bool> RevokeTokenByIdAsync(Guid tokenId)
        {
            var token = await _refreshTokenRepository.GetByIdAsync(tokenId);
            if (token == null || !token.IsActive) { return false; }
            token.Revoke();

            var success = await _refreshTokenRepository.UpdateAsync(token);
            if (success)
            {
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
            return success;
        }

        /// <summary>
        /// Revokes a specific refresh token by its raw string value (after hashing).
        /// </summary>
        /// <param name="refreshTokenString">The raw refresh token string.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning <c>true</c> if the token was successfully found and revoked, or if it didn't exist.</returns>
        public async Task<bool> RevokeTokenByStringAsync(string refreshTokenString)
        {
            var tokenHash = _tokenHasher.Hash(refreshTokenString);
            var tokenEntity = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (tokenEntity == null)
            {
                return true;
            }

            if (tokenEntity.IsActive)
            {
                tokenEntity.Revoke();
                var success = await _refreshTokenRepository.UpdateAsync(tokenEntity);

                if (success)
                {
                    await _unitOfWork.SaveChangesAsync(CancellationToken.None);
                }
                return success;
            }
            return true;
        }
    }
}