using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMService.Application.Features.User.Commands.UpdateUser
{
    /// <summary>
    /// Handles the update of user information.
    /// </summary>
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponseDto>
    {
        /// <summary>
        /// The user repository
        /// </summary>
        private readonly IUserRepository _userRepository;
        /// <summary>
        /// The role clone service
        /// </summary>
        private readonly IRoleCloneService _roleCloneService;
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserCommandHandler"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="roleCloneService">The role clone service.</param>
        /// <param name="logger">The logger.</param>
        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IRoleCloneService roleCloneService,
            ILogger<UpdateUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _roleCloneService = roleCloneService;
            _logger = logger;
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">User with ID {request.UserId} not found.</exception>
        public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing UpdateUserCommand for UserId: {UserId}", request.UserId);

            // Retrieve user from database
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new KeyNotFoundException($"User with ID {request.UserId} not found.");

            // Update basic information
            user.FullName = request.Dto.FullName ?? user.FullName;
            user.PhoneNumber = request.Dto.PhoneNumber ?? user.PhoneNumber;
            user.Email = request.Dto.Email ?? user.Email;
            user.IdentityNumber = request.Dto.IdentityNumber ?? user.IdentityNumber;
            user.Address = request.Dto.Address ?? user.Address;

            if (request.Dto.Gender.HasValue)
                user.Gender = request.Dto.Gender.Value;

            // Convert DateOfBirth(MM/ dd / yyyy format)
            if (!string.IsNullOrWhiteSpace(request.Dto.DateOfBirth) &&
                DateTime.TryParseExact(request.Dto.DateOfBirth, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out var dob))
            {
                user.DateOfBirth = DateOnly.FromDateTime(dob);
            }

            // If admin → update privileges (create custom role)
            if (request.IsAdmin && request.Dto.PrivilegeIds?.Any() == true)
            {
                _logger.LogInformation("Admin updating privileges for user {UserId}", user.UserId);


                var newRole = await _roleCloneService.CloneRoleWithPrivilegesAsync(
                    user,
                    request.Dto.PrivilegeIds,
                    cancellationToken
                );

                // Assign new RoleId to the user
                user.RoleId = newRole.RoleId;

                user.Role = null;

                _logger.LogInformation(
                    "✅ [UpdateUserCommandHandler] Admin updated privileges for user {UserId}. Assigned new RoleId: {RoleId}, RoleName: {RoleName}",
                    user.UserId,
                    newRole.RoleId,
                    newRole.RoleName
                );
            }

            // Save changes to database
            await _userRepository.UpdateAsync(user);

            // Reload user to include updated Role and Privileges
            var updatedUser = await _userRepository.GetByIdAsync(user.UserId);

            // Log event (AC04)
            _logger.LogInformation(
                "📘 [EventLog] User {UserId} information updated successfully. Updated by {Actor} | IsAdmin={IsAdmin}",
                user.UserId,
                request.IsAdmin ? "Admin" : "Self",
                request.IsAdmin
            );

            var privilegeIds = updatedUser?.Role?.Privileges?.Select(p => p.PrivilegeId).ToList() ?? new List<int>();
            var privilegeNames = updatedUser?.Role?.Privileges?.Select(p => p.PrivilegeName).ToList() ?? new List<string>();

            return new UserResponseDto
            {
                UserId = updatedUser!.UserId,
                FullName = updatedUser.FullName,
                PhoneNumber = updatedUser.PhoneNumber,
                Email = updatedUser.Email,
                Gender = updatedUser.Gender,
                IdentityNumber = updatedUser.IdentityNumber,
                DateOfBirth = updatedUser.DateOfBirth,
                Age = updatedUser.Age,
                Address = updatedUser.Address,
                RoleName = updatedUser.Role?.RoleName ?? "(none)",
                PrivilegeIds = privilegeIds,
                PrivilegeNames = privilegeNames
            };
        }
        
    }
}
