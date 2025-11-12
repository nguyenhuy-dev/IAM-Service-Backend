using IAMService.Application.DTOs;
using MediatR;
namespace IAMService.Application.Features.User.Commands.UpdateUser
{
    /// <summary>
    ///     The command used to update user information.
    /// </summary>
    /// <seealso cref="MediatR.IRequest&lt;IAMService.Application.DTOs.UserResponseDto&gt;" />
    /// <remarks>
    ///     This command supports both normal user self-updates and administrative updates.
    ///     - Normal users can update their personal information (e.g., name, phone, address).
    ///     - Admin users can also modify another user’s privileges by providing a custom list of privilege IDs.
    /// </remarks>
    public class UpdateUserCommand : IRequest<UserResponseDto>
    {
        /// <summary>
        ///     Gets or sets the unique identifier of the user being updated.
        /// </summary>
        /// <value>
        ///     The user identifier.
        /// </value>
        public Guid UserId { get; set; }
        /// <summary>
        ///     Gets or sets the dto.
        /// </summary>
        /// <value>
        ///     The dto.
        /// </value>
        public UpdateUserRequestDto Dto { get; set; } = default!;
        /// <summary>
        ///     Gets or sets a value indicating whether this instance is admin.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is admin; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdmin { get; set; }
    }
}
