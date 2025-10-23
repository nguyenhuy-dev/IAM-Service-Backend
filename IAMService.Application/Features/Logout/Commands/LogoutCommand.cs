using MediatR;
using System; // Required for Guid

namespace IAMService.Application.Features.Logout.Commands
{
    /// <summary>
    /// Represents a command to log out a user.
    /// </summary>
    /// <remarks>
    /// This command contains the necessary token and user information to invalidate an active session.
    /// It implements <see cref="IRequest{Unit}"/>, indicating it is a MediatR command that doesn't return a specific result.
    /// </remarks>
    public record LogoutCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets the value of the access token to be invalidated.
        /// </summary>
        public string AccessTokenValue { get; init; }

        /// <summary>
        /// Gets the value of the refresh token to be revoked.
        /// </summary>
        public string RefreshTokenValue { get; init; }

        /// <summary>
        /// Gets the unique identifier of the user who is logging out.
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutCommand"/> class.
        /// </summary>
        /// <param name="accessTokenValue">The access token value.</param>
        /// <param name="refreshTokenValue">The refresh token value.</param>
        /// <param name="userId">The ID of the user.</param>
        public LogoutCommand(string accessTokenValue, string refreshTokenValue, Guid userId)
        {
            AccessTokenValue = accessTokenValue;
            RefreshTokenValue = refreshTokenValue;
            UserId = userId;
        }
    }
}