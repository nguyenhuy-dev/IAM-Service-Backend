using IAMService.API.Common;
using IAMService.API.Controllers;
using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Features.Auth.Commands.Login;
using IAMService.Application.Features.Auth.Commands.Logout;
using IAMService.Application.Features.Auth.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace IAMService.API.Test
{
    [TestFixture]
    public class AuthsControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private AuthsController _controller = null!;
        private CancellationToken _token;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>();
            _controller = new AuthsController(_senderMock.Object);
            _token = CancellationToken.None;
        }

        #region Login Tests
        [Test]
        public async Task Login_ShouldReturnOk_WithValidCommand()
        {
            // Arrange
            var command = new LoginCommand("test@gmail.com", "Password123");
            var expectedResponse = new LoginResponse { Email = "test@gmail.com", AccessToken = "token123" };

            _senderMock.Setup(s => s.Send(command, _token))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(command, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<LoginResponse>;
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("Login successfully!"));
            Assert.That(apiResponse.Data, Is.EqualTo(expectedResponse));
        }
        #endregion

        #region Logout Tests
        [Test]
        public void Logout_ShouldThrowException_WhenUserIdIsInvalid()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
            }));
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(() => _controller.Logout(_token));
            Assert.That(ex!.Message, Does.Contain("Logout failed! User id invalid."));
        }

        [Test]
        public async Task Logout_ShouldReturnOk_WhenUserIdIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            _senderMock.Setup(s => s.Send(It.IsAny<LogoutCommand>(), _token))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Logout(_token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<bool>;
            Assert.That(apiResponse!.Data, Is.True);
            Assert.That(apiResponse.Message, Is.EqualTo("Logout successfully!"));
        }
        #endregion

        #region Refresh Token Tests
        [Test]
        public void RefreshToken_ShouldThrow_WhenBearerHeaderMissing()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.RefreshToken(_token));
            Assert.That(ex!.Message, Does.Contain("Missing Bearer header"));
        }

        [Test]
        public async Task RefreshToken_ShouldReturnOk_WhenBearerHeaderPresent()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Bearer"] = "old-token";
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var expectedResponse = new RefreshTokenResponse { AccessToken = "new-token" };
            _senderMock.Setup(s => s.Send(It.IsAny<RefreshTokenCommand>(), _token))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(_token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<RefreshTokenResponse>;
            Assert.That(apiResponse!.Data.AccessToken, Is.EqualTo("new-token"));
            Assert.That(apiResponse.Message, Is.EqualTo("Refresh token successfully!"));
            Assert.That(apiResponse.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }
        #endregion
    }
}
