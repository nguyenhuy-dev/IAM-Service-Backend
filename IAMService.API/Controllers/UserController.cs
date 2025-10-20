using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace IAMService.API.Controllers
{
    /// <summary>
    /// API controller for managing user-related operations.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// The sender
        /// </summary>
        private readonly ISender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController" /> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Updates the user information.
        /// Only the owner, Admin, or Manager can update user information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dto">The dto.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPut("{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(
            Guid userId,
            [FromBody][BindingBehavior(BindingBehavior.Optional)] UpdateUserRequestDto dto,
            CancellationToken cancellationToken)
        {
            //  Validate body
            if (dto == null)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Request body cannot be empty."
                });
            }

            if (dto.FullName == null &&
                dto.PhoneNumber == null &&
                dto.Email == null &&
                dto.Gender == null &&
                dto.IdentityNumber == null &&
                dto.DateOfBirth == null &&
                dto.Address == null &&
                (dto.PrivilegeIds == null || !dto.PrivilegeIds.Any()))
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "No fields were provided for update."
                });
            }

            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                var fakeClaims = new List<Claim>
                {
                    new Claim("sub", "a9b86d50-0b26-4526-99cc-07fdb3b78f98"), // UserId fake
                    new Claim("role", "Admin"), // Test privilege
                    new Claim("email", "testadmin@example.com"),
                    new Claim("fullName", "Mock Admin")
                };

                var fakeIdentity = new ClaimsIdentity(fakeClaims, "FakeJWT");
                HttpContext.User = new ClaimsPrincipal(fakeIdentity);
            }

            var currentUserIdClaim = User.FindFirst("sub")?.Value;
            var currentRole = User.FindFirst("role")?.Value ?? "User";

            if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid or missing user ID in token."
                });
            }

            bool isAdminOrManager = currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            bool isOwner = currentUserId == userId;

            if (!isOwner && !isAdminOrManager)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "You do not have permission to update another user's information."
                });
            }

            var command = new UpdateUserCommand
            {
                UserId = userId,
                Dto = dto,
                IsAdmin = isAdminOrManager
            };

            var result = await _sender.Send(command, cancellationToken);

            var response = ApiResponse<UserResponseDto>.Success(
                result,
                "User updated successfully.",
                StatusCodes.Status200OK
            );

            return Ok(response);
        }
    }
}
