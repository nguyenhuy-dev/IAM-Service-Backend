using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs.Auth.ForgotPassword;
using IAMService.Application.DTOs.Auth.Login;
using IAMService.Application.DTOs.Auth.ResetPassword;
using IAMService.Application.Features.ForgotPassword.Commands;
using IAMService.Application.Features.Login.Commands;
using IAMService.Application.Features.Logout.Commands;
using IAMService.Application.Features.ResetPassword.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IAMService.API.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related requests.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="sender">The MediatR sender instance for dispatching commands and queries.</param>
        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Authenticates a user based on their email and password.
        /// </summary>
        /// <param name="request">The login request containing email and password.</param>
        /// <returns>An action result containing the token response on success.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _sender.Send(new LoginCommand(request.Email, request.Password));

            if (result is TokenResponse tokenResponse)
            {
                var response = ApiResponse<TokenResponse>.Success(
                    tokenResponse,
                    "Login successful. Tokens and user information retrieved.",
                    StatusCodes.Status200OK
                );
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An internal error occurred: Login command returned an invalid response type."
            });
        }

        /// <summary>
        /// Logs out the user by invalidating the refresh token.
        /// </summary>
        /// <param name="request">The logout request containing the refresh token.</param>
        /// <returns>A 204 No Content response on successful logout.</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Logout()
        {
            
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Authentication header missing or format is invalid. Use: Authorization: Bearer <token>"
                });
            }

            
            var accessTokenValue = authHeader.Substring("Bearer ".Length).Trim();

            
            if (string.IsNullOrEmpty(accessTokenValue))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Access token value is empty."
                });
            }

            
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var refreshTokenValue = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenValue))
            {
                refreshTokenValue = null;
            }
            if (!Guid.TryParse(userIdString, out var userIdGuid))
            {
                
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid user ID found in token claims."
                });
            }

            var command = new LogoutCommand(
                accessTokenValue: accessTokenValue,
                refreshTokenValue: refreshTokenValue,
                userId: userIdGuid
            );

            await _sender.Send(command);
            Response.Cookies.Delete("refreshToken");
            return NoContent();
        }


        /// <summary>
        /// Initiates the password recovery process by sending a reset link to the user's email.
        /// Returns success regardless of user existence for security purposes.
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var command = new ForgotPasswordCommand { Email = request.Email };

            // Gửi Command
            await _sender.Send(command);

            return Ok(new
            {
                Message = "Nếu email hợp lệ, một liên kết đặt lại mật khẩu đã được gửi."
            });
        }

        /// <summary>
        /// Executes the password reset process using the unique token and new password.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Mật khẩu mới và mật khẩu xác nhận không khớp."
                });
            }
            var command = new ResetPasswordCommand
            {
                UserId = request.UserId,
                Token = request.Token,
                NewPassword = request.NewPassword
            };

            var success = await _sender.Send(command);

            if (success)
            {
                return Ok(new { Message = "Mật khẩu của bạn đã được đặt lại thành công." });
            }
            return BadRequest(new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Yêu cầu đặt lại mật khẩu không hợp lệ (token lỗi, hết hạn) hoặc tài khoản đang bị khóa."
            });
        }
    }
}