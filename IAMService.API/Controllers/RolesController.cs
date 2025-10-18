using IAMService.API.Common;
using IAMService.API.Middleware;
using IAMService.Application.DTOs;
using IAMService.Application.Features.Role.Commands.CreateRole;
using IAMService.Application.Features.Role.Commands.UpdateRole;
using IAMService.Application.Features.Role.Queries.GetRole;
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
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Updates the role using the specified role id
        /// </summary>
        /// <param name="roleId">The role id</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the action result</returns>
        [HttpPut("{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(
            [FromRoute] int roleId,
            [FromBody] UpdateRoleRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateRoleCommand(
                roleId,
                request.RoleName,
                request.RoleCode,
                request.Description,
                request.PrivilegeIds
            );
            
            var roleDto = await sender.Send(command, cancellationToken);
            
            var response = ApiResponse<RoleDto>.Success(
                roleDto, 
                "Role updated successfully.", 
                StatusCodes.Status200OK
            );
            
            return Ok(response);
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        /// PaginatedList<GetRoleRequest>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<GetRoleRequest>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRoles([FromQuery] GetRoleQuery query)
        {
            var result = await sender.Send(query);

            var message = "No Roles found.";
            if (result.Items.Count > 0)
            {
                message = "Get roles successfully.";
            }
            var response = ApiResponse<PaginatedList<GetRoleRequest>>.Success(
                result,
                message,
                StatusCodes.Status200OK
            );
            
            return Ok(response);

        }

    }
}
