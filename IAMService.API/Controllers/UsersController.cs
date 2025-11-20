using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.CreateUser;
using IAMService.Application.Features.User.Commands.DeleteUser;
using IAMService.Application.Features.User.Commands.UpdateUser;
using IAMService.Application.Features.User.Queries.GetAllUser;
using IAMService.Application.Features.User.Queries.ViewUserInformation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace IAMService.API.Controllers
{
    /// <summary>
    ///     Controller for user management operations
    ///     Handles user creation, retrieval, and management
    /// </summary>
    [ApiController, Route("api/[controller]"), Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly ISender _sender;

        /// <summary>
        ///     Constructor with dependency injection
        /// </summary>
        /// <param name="sender">MediatR sender instance</param>
        public UsersController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        ///     Creates a new user account
        /// </summary>
        /// <param name="command">User creation command containing all required information</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created user information</returns>
        [HttpPost, ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserCommand command,
            CancellationToken cancellationToken)
        {
            var userDto = await _sender.Send(command, cancellationToken);
            var message = command.IsPatient
                ? "Patient account created successfully. An email with login credentials has been sent."
                : "Employee account created successfully. Manual verification is required before activation.";
            var response = ApiResponse<UserDto>.Success(
                userDto,
                message,
                StatusCodes.Status201Created
            );
            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        ///     Updates the user information.
        ///     Only the owner, Admin, or Manager can update user information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dto">The dto.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [Authorize, HttpPut("{userId:guid}"), ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(
            Guid userId,
            [FromBody, BindingBehavior(BindingBehavior.Optional)] UpdateUserRequestDto dto,
            CancellationToken cancellationToken)
        {
            //  1. Validate the request body
            if (dto == null)
            {
                return BadRequest(new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Request body cannot be empty."
                });
            }

            //  2. Ensure at least one field is provided for update
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
                    StatusCode = 400,
                    Message = "No fields were provided for update."
                });
            }

            //  3. Verify authentication (JWT must be valid)
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "User is not authenticated or missing a valid token."
                });
            }

            // 4️⃣ Extract user claims from the JWT
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var currentRole =
                User.FindFirst(ClaimTypes.Role)?.Value
             ?? User.FindFirst("role")?.Value
             ?? User.FindFirst("roles")?.Value
             ?? "User";

            if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Invalid or missing user ID in token."
                });
            }

            // 5️⃣ Determine user permissions
            var isAdminOrManager =
                currentRole.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                currentRole.Equals("manager", StringComparison.OrdinalIgnoreCase);

            var isOwner = currentUserId == userId;

            if (!isOwner && !isAdminOrManager)
            {
                return StatusCode(403, new ErrorResponse
                {
                    StatusCode = 403,
                    Message = "You do not have permission to update another user's information."
                });
            }


            //  6. Send the command to the application layer via MediatR
            var command = new UpdateUserCommand
            {
                UserId = userId,
                Dto = dto,
                IsAdmin = isAdminOrManager
            };

            var result = await _sender.Send(command, cancellationToken);

            //  7. Return success response
            var response = ApiResponse<UserResponseDto>.Success(
                result,
                "User updated successfully."
            );

            return Ok(response);
        }

        /// <summary>
        ///     View detailed information of a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user to view.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        [HttpGet("{userId:guid}"), Authorize, ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid userId, CancellationToken cancellationToken)
        {
            // 🔹 Kiểm tra xác thực
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Unauthorized: missing or invalid JWT."
                });
            }

            // 🔹 Lấy thông tin từ JWT thật
            var currentUserIdClaim =
                User.FindFirst("sub")?.Value ??
                User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ??
                Request.Headers["X-User-Id"].FirstOrDefault();

            var currentRole = User.FindFirst("role")?.Value ?? "User";

            if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = 401,
                    Message = "Invalid or missing user ID in token."
                });
            }

            var currentUser = new CurrentUserDto
            {
                UserId = currentUserId,
                RoleName = currentRole
            };

            // 🔹 Tạo query và gửi qua MediatR
            var query = new ViewUserInformationQuery
            {
                TargetUserId = userId,
                CurrentUser = currentUser
            };

            var result = await _sender.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = "User not found or has been deleted."
                });
            }

            var response = ApiResponse<UserResponseDto>.Success(
                result,
                "User information retrieved successfully."
            );

            return Ok(response);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <summary>
        ///     Delete one user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}"), ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden), ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteUserCommand(id);

            var result = await _sender.Send(command, cancellationToken);

            var response = ApiResponse<bool>.Success(
                result,
                "User deleted successfully."
            );

            return Ok(response);
        }

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet, Authorize(Policy = "view_user"), ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status401Unauthorized), ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
    }
}
