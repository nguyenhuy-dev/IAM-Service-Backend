using FluentAssertions;
using FluentValidation;
using IAMService.Application.Exceptions;
using IAMService.Application.Features.Role.Commands.DeleteRole;
using IAMService.Application.Interfaces;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
namespace IAMService.Application.Test.Features.Role.Commands.DeleteRole
{
    /// <summary>
    ///     Unit Test for <see cref="DeleteRoleCommandHandler" />
    /// </summary>
    [TestFixture]
    public class DeleteRoleCommandHandlerTests
    {

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _userRepository = Substitute.For<IUserRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _handler = new DeleteRoleCommandHandler(_roleRepository, _userRepository, _unitOfWork);
        }
        /// <summary>
        ///     The role repository
        /// </summary>
        private IRoleRepository _roleRepository;
        /// <summary>
        ///     The user repository
        /// </summary>
        private IUserRepository _userRepository;
        /// <summary>
        ///     The unit of work
        /// </summary>
        private IUnitOfWork _unitOfWork;
        /// <summary>
        ///     The handler
        /// </summary>
        private DeleteRoleCommandHandler _handler;

        /// <summary>
        ///     Handles the valid role deletes role and reassigns users.
        /// </summary>
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

            var user1 = new Domain.Entities.User { UserId = Guid.NewGuid(), RoleId = roleId };
            var user2 = new Domain.Entities.User { UserId = Guid.NewGuid(), RoleId = roleId };
            var usersToUpdate = new List<Domain.Entities.User> { user1, user2 };

            _roleRepository.GetByIdAsync(roleId).Returns(roleToDelete);
            _roleRepository.GetByCodeAsync("READ_ONLY").Returns(readOnlyRole);
            _userRepository.GetByRoleIdAsync(roleId).Returns(usersToUpdate);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            user1.RoleId.Should().Be(readOnlyRoleId);
            user2.RoleId.Should().Be(readOnlyRoleId);
            _userRepository.Received(1).UpdateRange(Arg.Is<IEnumerable<Domain.Entities.User>>(u => u.Count() == 2));
            await _roleRepository.Received(1).DeleteAsync(roleToDelete);
            await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Handles the role not found throws not found exception.
        /// </summary>
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

        /// <summary>
        ///     Handles the default role throws validation exception.
        /// </summary>
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

        /// <summary>
        ///     Handles the read only role throws validation exception.
        /// </summary>
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

        /// <summary>
        ///     Handles the read only role not found throws invalid operation exception.
        /// </summary>
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
            _userRepository.GetByRoleIdAsync(roleId).Returns(new List<Domain.Entities.User>());

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("System configuration error: The 'READ_ONLY' role was not found.");
        }

        /// <summary>
        ///     Handles the no users with role deletes role successfully.
        /// </summary>
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
            _userRepository.GetByRoleIdAsync(roleId).Returns(new List<Domain.Entities.User>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _userRepository.Received(1).UpdateRange(Arg.Is<IEnumerable<Domain.Entities.User>>(u => !u.Any()));
            await _roleRepository.Received(1).DeleteAsync(roleToDelete);
            await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
        }

        /// <summary>
        ///     Handles the cancellation requested passes cancellation token.
        /// </summary>
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
            _userRepository.GetByRoleIdAsync(roleId).Returns(new List<Domain.Entities.User>());

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }
    }
}
