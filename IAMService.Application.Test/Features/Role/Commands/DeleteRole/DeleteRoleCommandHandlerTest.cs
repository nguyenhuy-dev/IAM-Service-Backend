using FluentAssertions;
using FluentValidation;
using IAMService.Application.Exceptions;
using IAMService.Application.Features.Role.Commands.DeleteRole;
using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace IAMService.Application.Tests.Features.Role.Commands.DeleteRole
{
    [TestFixture]
    public class DeleteRoleCommandHandlerTests
    {
        private IRoleRepository _roleRepository;
        private IUserRepository _userRepository;
        private IUnitOfWork _unitOfWork;
        private DeleteRoleCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _userRepository = Substitute.For<IUserRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _handler = new DeleteRoleCommandHandler(_roleRepository, _userRepository, _unitOfWork);
        }

        [Test]
        public async Task Handle_ValidRole_DeletesRoleAndReassignsUsers()
        {
            // Arrange
            var roleId = 1;
            var readOnlyRoleId = 2;
            var command = new DeleteRoleCommand(roleId);

            var roleToDelete = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "CUSTOM_ROLE",
                IsDefault = false
            };

            var readOnlyRole = new Domain.Entities.Role
            {
                RoleId = readOnlyRoleId,
                RoleCode = "READ_ONLY",
                IsDefault = true
            };

            var user1 = new User { UserId = Guid.NewGuid(), RoleId = roleId };
            var user2 = new User { UserId = Guid.NewGuid(), RoleId = roleId };
            var usersToUpdate = new List<User> { user1, user2 };

            _roleRepository.GetByIdAsync(roleId).Returns(roleToDelete);
            _roleRepository.GetByCodeAsync("READ_ONLY").Returns(readOnlyRole);
            _userRepository.GetByRoleIdAsync(roleId).Returns(usersToUpdate);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            user1.RoleId.Should().Be(readOnlyRoleId);
            user2.RoleId.Should().Be(readOnlyRoleId);
            _userRepository.Received(1).UpdateRange(Arg.Is<IEnumerable<User>>(u => u.Count() == 2));
            await _roleRepository.Received(1).DeleteAsync(roleToDelete);
            await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
        }

        [Test]
        public async Task Handle_RoleNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var roleId = 1;
            var command = new DeleteRoleCommand(roleId);

            _roleRepository.GetByIdAsync(roleId).ReturnsNull();

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*RoleId*");
        }

        [Test]
        public async Task Handle_DefaultRole_ThrowsValidationException()
        {
            // Arrange
            var roleId = 1;
            var command = new DeleteRoleCommand(roleId);

            var defaultRole = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "ADMIN",
                IsDefault = true
            };

            _roleRepository.GetByIdAsync(roleId).Returns(defaultRole);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Default roles cannot be deleted.");
        }

        [Test]
        public async Task Handle_ReadOnlyRole_ThrowsValidationException()
        {
            // Arrange
            var roleId = 1;
            var command = new DeleteRoleCommand(roleId);

            var readOnlyRole = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "READ_ONLY",
                IsDefault = false
            };

            _roleRepository.GetByIdAsync(roleId).Returns(readOnlyRole);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("The ReadOnly role cannot be deleted.");
        }

        [Test]
        public async Task Handle_ReadOnlyRoleNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var roleId = 1;
            var command = new DeleteRoleCommand(roleId);

            var roleToDelete = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "CUSTOM_ROLE",
                IsDefault = false
            };

            _roleRepository.GetByIdAsync(roleId).Returns(roleToDelete);
            _roleRepository.GetByCodeAsync("READ_ONLY").ReturnsNull();
            _userRepository.GetByRoleIdAsync(roleId).Returns(new List<User>());

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("System configuration error: The 'READ_ONLY' role was not found.");
        }

        [Test]
        public async Task Handle_NoUsersWithRole_DeletesRoleSuccessfully()
        {
            // Arrange
            var roleId = 1;
            var readOnlyRoleId = 2;
            var command = new DeleteRoleCommand(roleId);

            var roleToDelete = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "CUSTOM_ROLE",
                IsDefault = false
            };

            var readOnlyRole = new Domain.Entities.Role
            {
                RoleId = readOnlyRoleId,
                RoleCode = "READ_ONLY",
                IsDefault = true
            };

            _roleRepository.GetByIdAsync(roleId).Returns(roleToDelete);
            _roleRepository.GetByCodeAsync("READ_ONLY").Returns(readOnlyRole);
            _userRepository.GetByRoleIdAsync(roleId).Returns(new List<User>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _userRepository.Received(1).UpdateRange(Arg.Is<IEnumerable<User>>(u => !u.Any()));
            await _roleRepository.Received(1).DeleteAsync(roleToDelete);
            await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
        }

        [Test]
        public async Task Handle_CancellationRequested_PassesCancellationToken()
        {
            // Arrange
            var roleId = 1;
            var readOnlyRoleId = 2;
            var command = new DeleteRoleCommand(roleId);
            var cancellationToken = new CancellationToken(true);

            var roleToDelete = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "CUSTOM_ROLE",
                IsDefault = false
            };

            var readOnlyRole = new Domain.Entities.Role
            {
                RoleId = readOnlyRoleId,
                RoleCode = "READ_ONLY",
                IsDefault = true
            };

            _roleRepository.GetByIdAsync(roleId).Returns(roleToDelete);
            _roleRepository.GetByCodeAsync("READ_ONLY").Returns(readOnlyRole);
            _userRepository.GetByRoleIdAsync(roleId).Returns(new List<User>());

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }
    }
}