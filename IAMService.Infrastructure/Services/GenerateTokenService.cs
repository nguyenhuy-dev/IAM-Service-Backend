using IAMService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace IAMService.Infrastructure.Services
{
    /// <summary>
    ///     Provides services for generating JSON Web Tokens (JWT) and cryptographically secure refresh tokens.
    ///     This service encapsulates the logic for creating signed and time-limited access tokens based on configuration.
    /// </summary>
    public class GenerateTokenService : ITokenGenerator
    {
        /// <summary>
        ///     The duration, in seconds, until the Access Token expires.
        /// </summary>
        private readonly int _accessTokenLifeTimeSeconds;
        /// <summary>
        ///     The intended audience (recipient) of the token, typically the client application.
        /// </summary>
        private readonly string _audience;
        /// <summary>
        ///     The application configuration object used to read JWT settings.
        /// </summary>
        private readonly IConfiguration _configuration;
        /// <summary>
        ///     The issuer (source) of the token, typically the service's domain.
        /// </summary>
        private readonly string _issuer;
        /// <summary>
        ///     The secret key used to sign the JWT, ensuring its integrity.
        /// </summary>
        private readonly string _signingKey;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenerateTokenService" /> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if the 'Jwt:SigningKey' configuration value is missing.</exception>
        public GenerateTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Read configuration values once in the constructor
            _signingKey = _configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey not configured.");
            _issuer = _configuration["Jwt:Issuer"]!;
            _audience = _configuration["Jwt:Audience"]!;
            _accessTokenLifeTimeSeconds = _configuration.GetValue<int>("Jwt:AccessTokenLifetimeSeconds");
        }

        /// <summary>
        ///     Generates a new JWT Access Token containing user ID and roles.
        /// </summary>
        /// <param name="userId">The ID of the user for the 'sub' (subject) claim.</param>
        /// <param name="roleCode">Role code to be included in token claim</param>
        /// <returns>A tuple containing the generated token string and its validity duration in seconds.</returns>
        public (string Token, int ExpiresInSeconds) GenerateAccessToken(Guid userId, string roleCode)
        {
            var claims = new List<Claim>
            {
                // 'sub' claim: The principal (user ID) that is the subject of the JWT
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                // 'jti' claim: Unique token identifier
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add user roles to the claims if provided
            if (!string.IsNullOrEmpty(roleCode))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleCode));
            }

            // Define security key and signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Calculate the exact token expiry time
            var expires = DateTime.UtcNow.AddSeconds(_accessTokenLifeTimeSeconds);
            // The value to return to the client
            var expiresInSenconds = _accessTokenLifeTimeSeconds;

            // Create the Security Token
            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: expires, // 'exp' claim: Expiration time
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return the token string and its validity duration
            return (tokenString, expiresInSenconds);
        }

        /// <summary>
        ///     Generates a cryptographically random, Base64-encoded string for use as an Opaque Refresh Token.
        /// </summary>
        /// <returns>A random string of 64 bytes (approx. 86 Base64 characters).</returns>
        public string GenerateRefreshTokenString()
        {
            // Create a 64-byte random number array
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            // Convert to Base64 string to be URL-safe and easily transferable
            return Convert.ToBase64String(randomNumber);
        }
    }
}
