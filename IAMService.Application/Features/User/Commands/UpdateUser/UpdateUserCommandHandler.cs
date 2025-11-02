using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMService.Application.Features.User.Commands.UpdateUser
{
    /// <summary>
    /// Handles updating user information (basic info + privileges if admin).
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;IAMService.Application.Features.User.Commands.UpdateUser.UpdateUserCommand, IAMService.Application.DTOs.UserResponseDto&gt;" />
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
        private readonly IStringEncryptionService _stringEncryptionService;
        private readonly IUnitOfWork? _unitOfWork;

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
            : this(
                userRepository,
                roleCloneService,
                logger,
                new IAMService.Application.Services.NoOpStringEncryptionService(),
                null 
            )
        {
        }


        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IRoleCloneService roleCloneService,
            ILogger<UpdateUserCommandHandler> logger,
            IStringEncryptionService stringEncryptionService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleCloneService = roleCloneService;
            _logger = logger;
            _stringEncryptionService = stringEncryptionService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// User with ID {request.UserId} not found.
        /// or
        /// User with ID {user.UserId} not found after update.
        /// </exception>
        public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("➡️ Processing UpdateUserCommand for UserId: {UserId}", request.UserId);

            // 1️⃣ Retrieve user
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new KeyNotFoundException($"User with ID {request.UserId} not found.");

            // 2️⃣ Update basic info (encrypt sensitive fields before storing)
            user.FullName = request.Dto.FullName != null
                ? _stringEncryptionService.EncryptString(request.Dto.FullName)
                : user.FullName;

            user.PhoneNumber = request.Dto.PhoneNumber != null
                ? _stringEncryptionService.EncryptString(request.Dto.PhoneNumber)
                : user.PhoneNumber;

            user.Email = request.Dto.Email != null
                ? _stringEncryptionService.EncryptString(request.Dto.Email.Trim().ToLowerInvariant())
                : user.Email;

            user.IdentityNumber = request.Dto.IdentityNumber != null
                ? _stringEncryptionService.EncryptString(request.Dto.IdentityNumber)
                : user.IdentityNumber;

            user.Address = request.Dto.Address != null
                ? _stringEncryptionService.EncryptString(request.Dto.Address)
                : user.Address;

            if (request.Dto.Gender.HasValue)
                user.Gender = request.Dto.Gender.Value;

            // 3️⃣ Handle Date of Birth (convert "MM/dd/yyyy")
            if (!string.IsNullOrWhiteSpace(request.Dto.DateOfBirth) &&
                DateTime.TryParseExact(request.Dto.DateOfBirth, "MM/dd/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out var dob))
            {
                user.DateOfBirth = DateOnly.FromDateTime(dob);
            }

            // 4️⃣ Handle Privileges (Admin only)
            if (request.IsAdmin && request.Dto.PrivilegeIds?.Any() == true)
            {
                var currentPrivileges = user.Role?.Privileges?
                    .Select(p => p.PrivilegeId)
                    .OrderBy(x => x)
                    .ToList() ?? new List<int>();

                var newPrivileges = request.Dto.PrivilegeIds
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                bool privilegesChanged = !currentPrivileges.SequenceEqual(newPrivileges);

                if (privilegesChanged)
                {
                    _logger.LogInformation("⚙️ Privileges changed — cloning/reusing role for user {UserId}", user.UserId);

                    var newRole = await _roleCloneService.CloneRoleWithPrivilegesAsync(
                        user,
                        newPrivileges,
                        cancellationToken
                    );

                    // ✅ Assign RoleId only (to ensure EF picks up change)
                    user.RoleId = newRole.RoleId;
                    user.Role = null; // ⚠️ Critical to force EF to update RoleId

                    _logger.LogInformation("✅ User {UserId} now linked to RoleId={RoleId} ({RoleName})",
                        user.UserId, newRole.RoleId, newRole.RoleName);
                }
                else
                {
                    _logger.LogInformation("🔁 Privileges unchanged — keeping current role for {UserId}", user.UserId);
                }
            }

            // 5️⃣ Preserve role if missing
            if (user.RoleId == 0)
            {
                var existingUser = await _userRepository.GetByIdAsync(user.UserId);
                if (existingUser?.RoleId > 0)
                    user.RoleId = existingUser.RoleId;
            }

            // 6️⃣ Persist user (RoleId and basic info)
            await _userRepository.UpdateAsync(user);

            if (_unitOfWork != null)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("💾 User {UserId} successfully updated and saved.", user.UserId);

            // 7️⃣ Reload updated user
            var updatedUser = await _userRepository.GetByIdAsync(user.UserId)
                ?? throw new KeyNotFoundException($"User with ID {user.UserId} not found after update.");

            // 8️⃣ Map privileges
            var privilegeIds = updatedUser.Role?.Privileges?.Select(p => p.PrivilegeId).ToList() ?? new List<int>();
            var privilegeNames = updatedUser.Role?.Privileges?.Select(p => p.PrivilegeName).ToList() ?? new List<string>();

            // Decrypt sensitive fields for response
            var decryptedFullName = _stringEncryptionService.DecryptString(updatedUser.FullName);
            var decryptedPhone = _stringEncryptionService.DecryptString(updatedUser.PhoneNumber);
            var decryptedEmail = _stringEncryptionService.DecryptString(updatedUser.Email);
            var decryptedIdentity = _stringEncryptionService.DecryptString(updatedUser.IdentityNumber);
            var decryptedAddress = _stringEncryptionService.DecryptString(updatedUser.Address);

            return new UserResponseDto
            {
                UserId = updatedUser.UserId,
                FullName = decryptedFullName,
                PhoneNumber = decryptedPhone,
                Email = decryptedEmail,
                Gender = updatedUser.Gender,
                IdentityNumber = decryptedIdentity,
                DateOfBirth = updatedUser.DateOfBirth,
                Age = updatedUser.Age,
                Address = decryptedAddress,
                RoleName = updatedUser.Role?.RoleName ?? "(none)",
                PrivilegeIds = privilegeIds,
                PrivilegeNames = privilegeNames
            };
        }
    }
}
