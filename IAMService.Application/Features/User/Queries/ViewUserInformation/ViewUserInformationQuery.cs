using IAMService.Application.DTOs;
using MediatR;

namespace IAMService.Application.Features.User.Queries.ViewUserInformation
{
    /// <summary>
    /// Query to view detailed information for a specific user.
    /// </summary>
    /// <seealso cref="MediatR.IRequest&lt;IAMService.Application.DTOs.UserResponseDto&gt;" />
    public class ViewUserInformationQuery : IRequest<UserResponseDto>
    {
        /// <summary>
        /// Gets or sets the target user identifier.
        /// </summary>
        /// <value>
        /// The target user identifier.
        /// </value>
        public Guid TargetUserId { get; set; }
        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        public CurrentUserDto CurrentUser { get; set; } 
    }
}
