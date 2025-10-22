using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs.Auth.Login;
using IAMService.Application.Features.Login.Commands;
using IAMService.Application.Features.Logout.Commands;
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
            var accessTokenValue = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var refreshTokenValue = Request.Cookies["refreshToken"];
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
    }
}