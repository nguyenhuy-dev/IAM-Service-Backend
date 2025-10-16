using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs;
using IAMService.Application.Features.Role.Commands.CreateRole;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IAMService.API.Controllers
{
    /// <summary>
    /// The roles controller class
    /// </summary>
    /// <seealso cref="ControllerBase"/>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolesController(ISender sender) : ControllerBase
    {
        /// <summary>
        /// Creates the role using the specified command
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the action result</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole(
            [FromBody] CreateRoleCommand command,
            CancellationToken cancellationToken)
        {
            var roleDto = await sender.Send(command, cancellationToken);
            // Wrap the result in the standard success response object
            var response = ApiResponse<RoleDto>.Success(
                roleDto, 
                "Role created successfully.", 
                StatusCodes.Status201Created
            );
            return StatusCode(StatusCodes.Status201Created, response);
        }

    }
}
