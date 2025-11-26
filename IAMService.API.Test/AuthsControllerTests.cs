using IAMService.API.Common;
using IAMService.API.Controllers;
using IAMService.API.Middleware;
using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Exceptions;
using IAMService.Application.Features.Auth.Commands.Login;
using IAMService.Application.Features.Auth.Commands.Logout;
using IAMService.Application.Features.Auth.Commands.RefreshToken;
using IAMService.Application.Features.ForgotPassword.Commands;
using IAMService.Application.Features.ResetPassword.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace IAMService.API.Test
{
    [TestFixture]
    public class AuthsControllerTests
    {

        [SetUp]
        public void Setup()
        {
            _mockSender = new Mock<ISender>();
            _controller = new AuthsController(_mockSender.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }
        private Mock<ISender> _mockSender;
        private AuthsController _controller;
        [Test]
        public async Task Login_ReturnsOk_WhenValid()
        {
            var command = new LoginCommand("user", "pass");
            var loginResponse = new LoginResponse { AccessToken = "token" };

            _mockSender.Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(loginResponse);

            var result = await _controller.Login(command, CancellationToken.None) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var apiResponse = result.Value as ApiResponse<LoginResponse>;
            Assert.That(apiResponse!.Data!.AccessToken, Is.EqualTo("token"));
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }
        [Test]
        public void RefreshToken_ThrowsUnauthorized_WhenMissingBearer()
        {
            _controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _controller.RefreshToken(CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("Missing Bearer"));
        }

        [Test]
        public async Task RefreshToken_ReturnsOk_WhenValid()
        {
            _controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer oldToken";

            var refreshResponse = new RefreshTokenResponse { AccessToken = "newToken" };

            _mockSender.Setup(s => s.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshResponse);

            var result = await _controller.RefreshToken(CancellationToken.None) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var apiResponse = result.Value as ApiResponse<RefreshTokenResponse>;
            Assert.That(apiResponse!.Data.AccessToken, Is.EqualTo("newToken"));
        }
        [Test]
        public async Task ForgotPassword_ReturnsOk_WhenValid()
        {
            var request = new ForgotPasswordRequestDto { Email = "test@example.com" };

            _mockSender.Setup(s => s.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.ForgotPassword(request, CancellationToken.None) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var response = result.Value as ApiResponse<object>;
            Assert.That(response!.Message, Does.Contain("Password reset request processed"));
        }
        [Test]
        public async Task ResetPassword_ReturnsBadRequest_WhenPasswordsMismatch()
        {
            var request = new ResetPasswordRequestDto
            {
                NewPassword = "123",
                ConfirmPassword = "456"
            };

            var result = await _controller.ResetPassword(request, CancellationToken.None) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            var error = result.Value as ErrorResponse;
            Assert.That(error!.Message, Does.Contain("do not match"));
        }

        [Test]
        public async Task ResetPassword_ReturnsOk_WhenSuccess()
        {
            var request = new ResetPasswordRequestDto
            {
                UserId = Guid.NewGuid(),
                Token = "token",
                NewPassword = "123",
                ConfirmPassword = "123"
            };

            _mockSender.Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.ResetPassword(request, CancellationToken.None) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var response = result.Value as ApiResponse<object>;
            Assert.That(response!.Message, Does.Contain("Password reset successfully"));
        }

        [Test]
        public async Task ResetPassword_ReturnsBadRequest_WhenFailed()
        {
            var request = new ResetPasswordRequestDto
            {
                UserId = Guid.NewGuid(),
                Token = "token",
                NewPassword = "123",
                ConfirmPassword = "123"
            };

            _mockSender.Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.ResetPassword(request, CancellationToken.None) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            var error = result.Value as ErrorResponse;
            Assert.That(error!.Message, Does.Contain("failed"));
        }
        [Test]
        public void Logout_ThrowsNotFound_WhenUserIdInvalid()
        {
            _controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer " + GenerateJwtWithUserId("invalid-guid");

            var ex = Assert.ThrowsAsync<NotFoundException>(() =>
                _controller.Logout(CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("Logout failed"));
        }

        [Test]
        public async Task Logout_ReturnsOk_WhenSuccess()
        {
            var userGuid = Guid.NewGuid();
            _controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer " + GenerateJwtWithUserId(userGuid.ToString());

            _mockSender.Setup(s => s.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Logout(CancellationToken.None) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var apiResponse = result.Value as ApiResponse<bool>;
            Assert.That(apiResponse!.Data, Is.True);
            Assert.That(apiResponse.Message, Does.Contain("Logout successfully"));
        }
        private static string GenerateJwtWithUserId(string userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var token = new JwtSecurityToken(claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
