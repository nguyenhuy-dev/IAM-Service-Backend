using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs.AuthDTOs;
using IAMService.Application.Features.Auth.Commands.Login;
using IAMService.Application.Features.Auth.Commands.Logout;
using IAMService.Application.Features.Auth.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IAMService.API.Controllers;

/// <summary>
/// Authentication controllers.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
[Route("api/auth")]
[ApiController]
[Produces("application/json")]
public class AuthsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// The sender
    /// </summary>
    private readonly ISender _sender = sender;

    /// <summary>
    /// Logins the specified command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost("log-in")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var loginResponse = await _sender.Send(command, cancellationToken);

        var response = new ApiResponse<LoginResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Login successfully!",
            Data = loginResponse
        };

        return Ok(response);
    }

    /// <summary>
    /// Logouts the specified cancellation token.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException">userId - Logout failed! User id invalid.</exception>
    [HttpGet("log-out")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isUserIdGuid = Guid.TryParse(userId, out Guid userIdGuid);
        if (!isUserIdGuid)
            throw new ArgumentNullException(nameof(userId), "Logout failed! User id invalid.");

        var command = new LogoutCommand(userIdGuid);
        var isLogout = await _sender.Send(command, cancellationToken);

        var apiResponse = ApiResponse<bool>.Success(isLogout, "Logout successfully!", StatusCodes.Status200OK);

        return Ok(apiResponse);
    }

    /// <summary>
    /// Refreshes the token.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="System.UnauthorizedAccessException">Missing Bearer header...</exception>
    [HttpGet("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var oldAccessToken = HttpContext.Request.Headers["Bearer"].ToString();
        if (string.IsNullOrEmpty(oldAccessToken))
            throw new UnauthorizedAccessException("Missing Bearer header...");

        var refreshTokenCommand = new RefreshTokenCommand(oldAccessToken);
        var refreshTokenResponse = await _sender.Send(refreshTokenCommand, cancellationToken);

        var response = new ApiResponse<RefreshTokenResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Refresh token successfully!",
            Data = refreshTokenResponse
        };

        return Ok(response);
    }
}
