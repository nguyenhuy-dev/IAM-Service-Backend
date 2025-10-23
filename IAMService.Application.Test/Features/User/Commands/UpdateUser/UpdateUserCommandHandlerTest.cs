using FluentAssertions;
using IAMService.Application.DTOs; // contains UpdateUserRequestDto, UserResponseDto, ...
using IAMService.Application.Features.User.Commands.UpdateUser;
using IAMService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IAMService.Application.Test.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Unit tests for the UpdateUserCommandHandler class.
    /// </summary>
    [TestFixture]
    public class UpdateUserCommandHandlerTests
    {
        /// <summary>
        /// The role clone service
        /// </summary>
        private IRoleCloneService _roleCloneService;
        /// <summary>
        /// The user repository
        /// </summary>
        private IUserRepository _userRepository;
        /// <summary>
        /// The logger
        /// </summary>
        private ILogger<UpdateUserCommandHandler> _logger;
        /// <summary>
        /// The handler
        /// </summary>
        private UpdateUserCommandHandler _handler;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _roleClone_service_fix(); // helper to avoid name clash in editor if needed

            _roleCloneService = Substitute.For<IRoleCloneService>();
            _logger = Substitute.For<ILogger<UpdateUserCommandHandler>>();

            _handler = new UpdateUserCommandHandler(
                _userRepository,
                _roleCloneService,
                _logger
            );
        }

        // helper (no-op) in case your editor flagged the previous symbol name
        /// <summary>
        /// Roles the clone service fix.
        /// </summary>
        private void _roleClone_service_fix() { /* no-op */ }

        /// <summary>
        /// Handles the valid user updates basic information successfully.
        /// </summary>
        [Test]
        public async Task Handle_ValidUser_UpdatesBasicInfoSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Use fully-qualified domain entity types to avoid namespace/type conflicts
            var existingUser = new global::IAMService.Domain.Entities.User
            {
                UserId = userId,
                FullName = "Old Name",
                PhoneNumber = "0000000000",
                Email = "old@example.com",
                Gender = true,
                Address = "Old Address",
                IdentityNumber = "111111111111",
                DateOfBirth = new DateOnly(1990, 1, 1),
                Role = new global::IAMService.Domain.Entities.Role
                {
                    RoleId = 1,
                    RoleName = "Employee"
                },
                RoleId = 1
            };

            var command = new UpdateUserCommand
            {
                UserId = userId,
                IsAdmin = false,
                Dto = new UpdateUserRequestDto
                {
                    FullName = "New Name",
                    PhoneNumber = "0999999999",
                    Email = "new@example.com",
                    Gender = false,
                    Address = "New Address",
                    IdentityNumber = "222222222222",
                    DateOfBirth = "05/10/1995"
                }
            };

            _userRepository.GetByIdAsync(userId).Returns(existingUser);
            _userRepository.UpdateAsync(Arg.Any<global::IAMService.Domain.Entities.User>()).Returns(Task.CompletedTask);

            // After update, repository returns updated user (simulate reload)
            var reloadedUser = new global::IAMService.Domain.Entities.User
            {
                UserId = userId,
                FullName = command.Dto.FullName,
                PhoneNumber = command.Dto.PhoneNumber,
                Email = command.Dto.Email,
                Gender = command.Dto.Gender.Value,
                Address = command.Dto.Address,
                IdentityNumber = command.Dto.IdentityNumber,
                DateOfBirth = DateOnly.ParseExact(command.Dto.DateOfBirth, "MM/dd/yyyy", null),
                Role = existingUser.Role,
                RoleId = existingUser.RoleId
            };
            _userRepository.GetByIdAsync(userId).Returns(existingUser, reloadedUser);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("New Name");
            result.Email.Should().Be("new@example.com");
            result.Address.Should().Be("New Address");
            result.IdentityNumber.Should().Be("222222222222");
            result.DateOfBirth.Should().Be(reloadedUser.DateOfBirth);

            await _userRepository.Received(1).UpdateAsync(Arg.Is<global::IAMService.Domain.Entities.User>(u =>
                u.FullName == "New Name" &&
                u.Email == "new@example.com" &&
                u.Address == "New Address"));
        }

        /// <summary>
        /// Handles the admin updates privileges clones role.
        /// </summary>
        [Test]
        public async Task Handle_AdminUpdatesPrivileges_ClonesRole()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var oldRole = new global::IAMService.Domain.Entities.Role
            {
                RoleId = 1,
                RoleName = "Employee",
                Privileges = new List<global::IAMService.Domain.Entities.Privilege>
                {
                    new global::IAMService.Domain.Entities.Privilege { PrivilegeId = 1 } // don't set PrivilegeName if setter is inaccessible
                }
            };

            var user = new global::IAMService.Domain.Entities.User
            {
                UserId = userId,
                Role = oldRole,
                RoleId = 1,
                FullName = "Admin User"
            };

            var command = new UpdateUserCommand
            {
                UserId = userId,
                IsAdmin = true,
                Dto = new UpdateUserRequestDto
                {
                    PrivilegeIds = new List<int> { 1, 2 }
                }
            };

            _userRepository.GetByIdAsync(userId).Returns(user);

            var newRole = new global::IAMService.Domain.Entities.Role
            {
                RoleId = 99,
                RoleName = "Employee (Custom)",
                Privileges = new List<global::IAMService.Domain.Entities.Privilege>
                {
                    new global::IAMService.Domain.Entities.Privilege { PrivilegeId = 1 },
                    new global::IAMService.Domain.Entities.Privilege { PrivilegeId = 2 }
                }
            };

            _roleCloneService
                .CloneRoleWithPrivilegesAsync(Arg.Is<global::IAMService.Domain.Entities.User>(u => u.UserId == userId),
                    Arg.Any<List<int>>(),
                    Arg.Any<CancellationToken>())
                .Returns(newRole);

            _userRepository.UpdateAsync(Arg.Any<global::IAMService.Domain.Entities.User>()).Returns(Task.CompletedTask);

            // After update reload returns role with privileges
            var reloaded = new global::IAMService.Domain.Entities.User
            {
                UserId = userId,
                FullName = user.FullName,
                Role = newRole,
                RoleId = newRole.RoleId
            };
            _userRepository.GetByIdAsync(userId).Returns(user, reloaded);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.RoleName.Should().Be("Employee (Custom)");
            result.PrivilegeIds.Should().BeEquivalentTo(new List<int> { 1, 2 });

            await _roleCloneService.Received(1)
                .CloneRoleWithPrivilegesAsync(Arg.Is<global::IAMService.Domain.Entities.User>(u => u.UserId == userId),
                    Arg.Any<List<int>>(),
                    Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Handles the admin privileges unchanged no clone happens.
        /// </summary>
        [Test]
        public async Task Handle_AdminPrivilegesUnchanged_NoCloneHappens()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var privileges = new List<global::IAMService.Domain.Entities.Privilege>
            {
                new global::IAMService.Domain.Entities.Privilege { PrivilegeId = 1 },
                new global::IAMService.Domain.Entities.Privilege { PrivilegeId = 2 }
            };

            var user = new global::IAMService.Domain.Entities.User
            {
                UserId = userId,
                FullName = "Same Privileges User",
                Role = new global::IAMService.Domain.Entities.Role
                {
                    RoleId = 1,
                    RoleName = "Employee",
                    Privileges = privileges
                },
                RoleId = 1
            };

            var command = new UpdateUserCommand
            {
                UserId = userId,
                IsAdmin = true,
                Dto = new UpdateUserRequestDto
                {
                    PrivilegeIds = new List<int> { 1, 2 } // same as before
                }
            };

            _userRepository.GetByIdAsync(userId).Returns(user);
            _userRepository.UpdateAsync(Arg.Any<global::IAMService.Domain.Entities.User>()).Returns(Task.CompletedTask);
            _userRepository.GetByIdAsync(userId).Returns(user, user); // reload same user

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _roleCloneService.DidNotReceiveWithAnyArgs()
                .CloneRoleWithPrivilegesAsync(default!, default!, default!);

            result.Should().NotBeNull();
            result.RoleName.Should().Be("Employee");
        }

        /// <summary>
        /// Handles the user not found throws key not found exception.
        /// </summary>
        [Test]
        public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                Dto = new UpdateUserRequestDto { FullName = "Non Existent" }
            };

            _userRepository.GetByIdAsync(command.UserId).Returns((global::IAMService.Domain.Entities.User?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{command.UserId}*not found*");
        }

        /// <summary>
        /// Handles the user missing after update throws key not found exception.
        /// </summary>
        [Test]
        public async Task Handle_UserMissingAfterUpdate_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new global::IAMService.Domain.Entities.User { UserId = userId, FullName = "Before Update" };

            var command = new UpdateUserCommand
            {
                UserId = userId,
                Dto = new UpdateUserRequestDto { FullName = "After Update" }
            };

            // first call returns user, second (after update) returns null
            _userRepository.GetByIdAsync(userId).Returns(user, (global::IAMService.Domain.Entities.User?)null);
            _userRepository.UpdateAsync(Arg.Any<global::IAMService.Domain.Entities.User>()).Returns(Task.CompletedTask);

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"*{userId}*not found after update*");
        }
    }
}
