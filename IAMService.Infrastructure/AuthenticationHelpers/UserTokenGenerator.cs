using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace IAMService.Infrastructure.AuthenticationHelpers
{
    /// <summary>
    ///     Token generator service.
    /// </summary>
    /// <seealso cref="IAMService.Application.Interfaces.AuthenticationServices.IUserTokenGenerator" />
    public class UserTokenGenerator(IConfiguration configuration, IAuthRepository authRepository) : IUserTokenGenerator
    {

        /// <summary>
        ///     The authentication repository
        /// </summary>
        private readonly IAuthRepository _authRepository = authRepository;
        /// <summary>
        ///     The configuration
        /// </summary>
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        ///     Generates the token asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<JwtToken> GenerateTokenAsync(User user, CancellationToken cancellationToken)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SigningKey"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.RoleCode)
            };

            var accessToken = new JwtSecurityToken(
                jwtSection["Issuer"],
                jwtSection["Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["AccessTokenLifetimeMinutes"]!)),
                signingCredentials: creds
            );

            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            var refreshTokenString = Guid.CreateVersion7().ToString();

            JwtToken jwtToken = new JwtToken
            {
                Id = Guid.CreateVersion7(),
                AccessToken = accessTokenString,
                RefreshToken = refreshTokenString,
                ReTokenExpireAt = DateTime.UtcNow.AddDays(double.Parse(jwtSection["RefreshTokenLifetimeDays"]!)),
                IsRevoked = false,
                CreateAt = DateTime.UtcNow,
                UserId = user.UserId
            };

            await _authRepository.AddJwtToken(jwtToken, cancellationToken);

            return jwtToken;
        }
    }
}
