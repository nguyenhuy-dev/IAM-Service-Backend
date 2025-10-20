using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.UpdateUser;
using IAMService.Application.Features.User.Queries.ViewUserInformation;
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

            // Ensure that at least one field is provided for updating
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

            // If no authentication context is found, create mock claims for testing
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                var fakeClaims = new List<Claim>
                {
                    new Claim("sub", "a9b86d50-0b26-4526-99cc-07fdb3b78f98"), // UserId fake
                    new Claim("role", "Admin"), // Test privilege
                    new Claim("email", "testadmin@example.com"),
                    new Claim("fullName", "Mock Admin")
                };

                // Assign the fake identity to the current user context
                var fakeIdentity = new ClaimsIdentity(fakeClaims, "FakeJWT");
                HttpContext.User = new ClaimsPrincipal(fakeIdentity);
            }

            // Extract the current user's ID and role from claims
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

            // Determine if the user has admin or manager privileges
            bool isAdminOrManager = currentRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || currentRole.Equals("Manager", StringComparison.OrdinalIgnoreCase);

            // Determine if the current user is updating their own account
            bool isOwner = currentUserId == userId;

            if (!isOwner && !isAdminOrManager)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "You do not have permission to update another user's information."
                });
            }

            // Create the update command and pass the necessary data
            var command = new UpdateUserCommand
            {
                UserId = userId,
                Dto = dto,
                IsAdmin = isAdminOrManager
            };

            // Send the command to the application layer using MediatR
            var result = await _sender.Send(command, cancellationToken);

            var response = ApiResponse<UserResponseDto>.Success(
                result,
                "User updated successfully.",
                StatusCodes.Status200OK
            );

            return Ok(response);
        }
        /// <summary>
        /// View detailed information of a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user to view.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                var fakeClaims = new List<Claim>
        {
            new Claim("sub", "a9b86d50-0b26-4526-99cc-07fdb3b78f98"), 
            new Claim("role", "Admin"), 
            new Claim("email", "mockuser@example.com"),
            new Claim("fullName", "Mock Admin User")
        };

                var fakeIdentity = new ClaimsIdentity(fakeClaims, "FakeJWT");
                HttpContext.User = new ClaimsPrincipal(fakeIdentity);
            }

            // Extract user ID and role from claims
            var currentUserIdClaim = User.FindFirst("sub")?.Value;
            var currentRole = User.FindFirst("role")?.Value ?? "User";

            // Validate user ID from token
            if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid or missing user ID in token."
                });
            }

            // Create a DTO representing the current authenticated user
            var currentUser = new CurrentUserDto
            {
                UserId = currentUserId,
                RoleName = currentRole
            };

            // Create the query to retrieve the target user’s information
            var query = new ViewUserInformationQuery
            {
                TargetUserId = userId,
                CurrentUser = currentUser
            };

            // Send the query to the application layer via MediatR
            var result = await _sender.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found or has been deleted."
                });
            }

            // Return the retrieved user information in a standard API response
            var response = ApiResponse<UserResponseDto>.Success(
                result,
                "User information retrieved successfully.",
                StatusCodes.Status200OK
            );

            return Ok(response);
        }

    }
}
    