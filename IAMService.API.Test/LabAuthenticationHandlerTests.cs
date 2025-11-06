using IAMService.API.Middleware.Authentication;
using IAMService.Application.Interfaces.AuthenticationServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace IAMService.API.Test
{
    [TestFixture]
    public class LabAuthenticationHandlerTests
    {
        private Mock<IAuthRepository> _authRepositoryMock = null!;
        private Mock<ILoggerFactory> _loggerFactoryMock = null!;
        private DefaultHttpContext _httpContext = null!;
        private LabAuthenticationHandler _handler = null!;
        private IOptionsMonitor<LabAuthenticationSchemeOptions> _options = null!;
        private UrlEncoder _encoder = null!;

        private const string SecretKey = "ThisIsASecretKey1234567890123456"; // 32 bytes

        [SetUp]
        public void Setup()
        {
            _authRepositoryMock = new Mock<IAuthRepository>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(Mock.Of<ILogger<LabAuthenticationHandler>>());

            _encoder = UrlEncoder.Default;
            _httpContext = new DefaultHttpContext();

            var options = Options.Create(new LabAuthenticationSchemeOptions
            {
                IssuerSigningKey = SecretKey,
                ValidIssuer = "test-issuer",
                ValidAudience = "test-audience"
            });

            var optionsMonitor = Mock.Of<IOptionsMonitor<LabAuthenticationSchemeOptions>>(x => x.Get(It.IsAny<string>()) == options.Value);
            _options = optionsMonitor;

            _handler = new LabAuthenticationHandler(_authRepositoryMock.Object, _options, _loggerFactoryMock.Object, _encoder);
            _handler.InitializeAsync(new AuthenticationScheme("TestScheme", null, typeof(LabAuthenticationHandler)), _httpContext);
        }

        private string GenerateValidJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = "test-issuer",
                Audience = "test-audience",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Test]
        public async Task HandleAuthenticateAsync_ShouldReturnNoResult_WhenPassedPath()
        {
            _httpContext.Request.Path = "/api/login";

            var result = await _handler.AuthenticateAsync();

            Assert.That(result.None, Is.True);
        }

        [Test]
        public async Task HandleAuthenticateAsync_ShouldFail_WhenMissingHeader()
        {
            _httpContext.Request.Path = "/api/patient";

            var result = await _handler.AuthenticateAsync();

            Assert.That(result.Failure, Is.Not.Null);
            Assert.That(result.Failure!.Message, Does.Contain("Missing Bearer header"));
        }

        [Test]
        public async Task HandleAuthenticateAsync_ShouldSucceed_WhenTokenAndDbValid()
        {
            _httpContext.Request.Path = "/api/patient";

            var validToken = GenerateValidJwtToken();
            _httpContext.Request.Headers.Authorization = $"Bearer {validToken}";

            _authRepositoryMock.Setup(x => x.CheckValidToken(It.IsAny<string>()))
                               .ReturnsAsync(true);

            var result = await _handler.AuthenticateAsync();

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Principal, Is.Not.Null);
            Assert.That(result.Ticket, Is.Not.Null);
        }

        [Test]
        public async Task HandleAuthenticateAsync_ShouldFail_WhenTokenNotInDb()
        {
            _httpContext.Request.Path = "/api/patient";

            var validToken = GenerateValidJwtToken();
            _httpContext.Request.Headers.Authorization = $"Bearer {validToken}";

            _authRepositoryMock.Setup(x => x.CheckValidToken(It.IsAny<string>()))
                               .ReturnsAsync(false);

            var result = await _handler.AuthenticateAsync();

            Assert.That(result.Failure, Is.Not.Null);
            Assert.That(result.Failure!.Message, Does.Contain("Verify token unsuccessfully."));
        }

        [Test]
        public async Task HandleAuthenticateAsync_ShouldFail_WhenTokenInvalid()
        {
            _httpContext.Request.Path = "/api/patient";
            _httpContext.Request.Headers.Authorization = "Bearer invalid.token.value";

            _authRepositoryMock.Setup(x => x.CheckValidToken(It.IsAny<string>()))
                               .ReturnsAsync(true);

            var result = await _handler.AuthenticateAsync();

            Assert.That(result.Failure, Is.Not.Null);
            Assert.That(result.Failure!.Message, Does.Contain("Verify token unsuccessfully."));
        }

        [Test]
        public async Task HandleChallengeAsync_ShouldReturnJson401_WhenNotStarted()
        {
            _httpContext.Response.Body = new MemoryStream();

            await _handler.ChallengeAsync(new AuthenticationProperties());

            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(responseBody, Does.Contain("Unauthorized access"));
        }

        [Test]
        public async Task HandleChallengeAsync_ShouldDoNothing_WhenResponseStarted()
        {
            _httpContext.Response.Body = new MemoryStream();
            await _httpContext.Response.StartAsync();

            await _handler.ChallengeAsync(new AuthenticationProperties());

            Assert.That(_httpContext.Response.HasStarted, Is.False);
        }
    }
}
