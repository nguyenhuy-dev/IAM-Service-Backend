using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.UpdateUser;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.EventBus;
using IAMService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
namespace IAMService.Application.Test.Features.User.Commands.UpdateUser
{
    [TestFixture]
    public class UpdateUserCommandHandlerTests
    {

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleCloneServiceMock = new Mock<IRoleCloneService>();
            _loggerMock = new Mock<ILogger<UpdateUserCommandHandler>>();
            _encryptionServiceMock = new Mock<IStringEncryptionService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            _handler = new UpdateUserCommandHandler(
                _userRepositoryMock.Object,
                _roleCloneServiceMock.Object,
                _loggerMock.Object,
                _encryptionServiceMock.Object,
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _eventPublisherMock.Object
            );
        }
        private Mock<IUserRepository> _userRepositoryMock = null!;
        private Mock<IRoleCloneService> _roleCloneServiceMock = null!;
        private Mock<ILogger<UpdateUserCommandHandler>> _loggerMock = null!;
        private Mock<IStringEncryptionService> _encryptionServiceMock = null!;
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IEventPublisher> _eventPublisherMock = null!;
        private UpdateUserCommandHandler _handler = null!;

        private Domain.Entities.User CreateUser(Guid? id = null)
        {
            return new Domain.Entities.User
            {
                UserId = id ?? Guid.NewGuid(),
                FullName = "EncryptedName",
                PhoneNumber = "EncryptedPhone",
                Email = "EncryptedEmail",
                IdentityNumber = "EncryptedIdentity",
                Address = "EncryptedAddress",
                Gender = true,
                DateOfBirth = new DateOnly(2000, 1, 1),
                RoleId = 1,
                Role = new Domain.Entities.Role
                {
                    RoleId = 1,
                    RoleName = "RoleName",
                    Privileges = new List<Privilege>
                    {
                        new Privilege { PrivilegeId = 1, PrivilegeName = "Privilege1" }
                    }
                }
            };
        }

        private UpdateUserCommand CreateCommand(
            Guid? userId = null,
            bool isAdmin = false,
            string dob = "01/01/2000",
            List<int>? privilegeIds = null
        )
        {
            return new UpdateUserCommand
            {
                UserId = userId ?? Guid.NewGuid(),
                IsAdmin = isAdmin,
                Dto = new UpdateUserRequestDto
                {
                    FullName = "NewName",
                    PhoneNumber = "NewPhone",
                    Email = "newemail@example.com",
                    IdentityNumber = "123456789012",
                    Address = "NewAddress",
                    DateOfBirth = dob,
                    PrivilegeIds = privilegeIds
                }
            };
        }

        // ----------------------------------------------------------------
        // TEST CASES
        // ----------------------------------------------------------------

        [Test]
        public void Handle_UserNotFound_ShouldThrowKeyNotFound()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync((Domain.Entities.User?)null);

            var cmd = CreateCommand();

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public void Handle_DuplicateEmail_ShouldThrowInvalidOperationException()
        {
            var user = CreateUser();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, false))
                .ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.DecryptString(user.Email))
                .Returns("old@example.com");

            _userRepositoryMock.Setup(x =>
                    x.ExistsByEmailAsync("newemail@example.com", user.UserId))
                .ReturnsAsync(true);

            var cmd = CreateCommand(user.UserId);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public void Handle_DuplicateIdentity_ShouldThrowInvalidOperationException()
        {
            var user = CreateUser();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, false))
                .ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.DecryptString(user.IdentityNumber))
                .Returns("111111111111");

            _userRepositoryMock.Setup(x =>
                    x.ExistsByIdentityNumberAsync("123456789012", user.UserId))
                .ReturnsAsync(true);

            var cmd = CreateCommand(user.UserId);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UpdateBasicFields_ShouldUpdateCorrectly()
        {
            var user = CreateUser();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, false))
                .ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.EncryptString(It.IsAny<string>()))
                .Returns((string s) => s);

            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>()))
                .Returns((string s) => s);

            _userRepositoryMock.Setup(x =>
                    x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);

            _userRepositoryMock.Setup(x =>
                    x.ExistsByIdentityNumberAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);

            var cmd = CreateCommand(user.UserId);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.That(result.FullName, Is.EqualTo(cmd.Dto.FullName));
            Assert.That(result.Email, Is.EqualTo(cmd.Dto.Email));

            _userRepositoryMock.Verify(x =>
                x.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Once);
        }

        [Test]
        public async Task Handle_ChangePrivileges_ShouldCloneRole()
        {
            var user = CreateUser();

            var newRole = new Domain.Entities.Role
            {
                RoleId = 99,
                RoleName = "ClonedRole"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, false))
                .ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>()))
                .Returns((string s) => s);

            _userRepositoryMock.Setup(x =>
                    x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);

            _roleCloneServiceMock.Setup(x =>
                    x.CloneRoleWithPrivilegesAsync(
                        user,
                        It.IsAny<List<int>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(newRole);

            var cmd = CreateCommand(
                user.UserId,
                true,
                privilegeIds: new List<int> { 2, 3 }
            );

            await _handler.Handle(cmd, CancellationToken.None);

            Assert.That(user.RoleId, Is.EqualTo(newRole.RoleId));
        }

        [Test]
        public async Task Handle_PrivilegesUnchanged_ShouldNotCloneRole()
        {
            var user = CreateUser();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, false))
                .ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>()))
                .Returns((string s) => s);

            _userRepositoryMock.Setup(x =>
                    x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);

            var cmd = CreateCommand(
                user.UserId,
                true,
                privilegeIds: new List<int> { 1 }
            );

            await _handler.Handle(cmd, CancellationToken.None);

            _roleCloneServiceMock.Verify(x =>
                    x.CloneRoleWithPrivilegesAsync(
                        It.IsAny<Domain.Entities.User>(),
                        It.IsAny<List<int>>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
