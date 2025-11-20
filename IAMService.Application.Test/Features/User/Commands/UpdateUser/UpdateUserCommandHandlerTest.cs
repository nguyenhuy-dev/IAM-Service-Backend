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

        private Domain.Entities.User CreateUser(Guid? userId = null)
        {
            var id = userId ?? Guid.NewGuid();
            return new Domain.Entities.User
            {
                UserId = id,
                FullName = "EncryptedName",
                PhoneNumber = "EncryptedPhone",
                Email = "EncryptedEmail",
                IdentityNumber = "EncryptedId",
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

        private UpdateUserCommand CreateCommand(Guid? userId = null, bool isAdmin = false, string dob = "01/01/2000", List<int>? privilegeIds = null)
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

        [Test]
        public void Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync((Domain.Entities.User?)null);

            var command = CreateCommand();

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UpdatesBasicFields_DecryptsAndReturnsDto()
        {
            var user = CreateUser();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.EncryptString(It.IsAny<string>())).Returns((string s) => s);
            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>())).Returns((string s) => s);

            var command = CreateCommand(user.UserId);

            var result = await _handler.Handle(command, CancellationToken.None);

            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.User>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);

            Assert.That(result.FullName, Is.EqualTo(command.Dto.FullName));
            Assert.That(result.Email, Is.EqualTo(command.Dto.Email));
            Assert.That(result.DateOfBirth, Is.EqualTo(new DateOnly(2000, 1, 1)));
        }

        [Test]
        public async Task Handle_ChangesPrivileges_ClonesRoleAndUpdates()
        {
            var user = CreateUser();
            var newRole = new Domain.Entities.Role { RoleId = 2, RoleName = "ClonedRole" };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _roleCloneServiceMock.Setup(x => x.CloneRoleWithPrivilegesAsync(
                    It.IsAny<Domain.Entities.User>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newRole);

            _encryptionServiceMock.Setup(x => x.EncryptString(It.IsAny<string>())).Returns((string s) => s);
            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>())).Returns((string s) => s);

            var command = CreateCommand(user.UserId, true, privilegeIds: new List<int> { 2, 3 });

            await _handler.Handle(command, CancellationToken.None);

            Assert.That(user.RoleId, Is.EqualTo(newRole.RoleId));
        }

        [Test]
        public async Task Handle_PrivilegesUnchanged_DoesNotCloneRole()
        {
            var user = CreateUser();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, It.IsAny<bool>())).ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.EncryptString(It.IsAny<string>())).Returns((string s) => s);
            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>())).Returns((string s) => s);

            var command = CreateCommand(user.UserId, true, privilegeIds: new List<int> { 1 });

            await _handler.Handle(command, CancellationToken.None);

            _roleCloneServiceMock.Verify(x => x.CloneRoleWithPrivilegesAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_InvalidDateOfBirth_Ignores()
        {
            var user = CreateUser();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, It.IsAny<bool>())).ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.EncryptString(It.IsAny<string>())).Returns((string s) => s);
            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>())).Returns((string s) => s);

            var command = CreateCommand(user.UserId, dob: "invalid-date");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.DateOfBirth, Is.EqualTo(user.DateOfBirth));
        }

        [Test]
        public async Task Handle_NoPrivilegesNoAdmin_NoRoleChange()
        {
            var user = CreateUser();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.UserId, It.IsAny<bool>())).ReturnsAsync(user);

            _encryptionServiceMock.Setup(x => x.EncryptString(It.IsAny<string>())).Returns((string s) => s);
            _encryptionServiceMock.Setup(x => x.DecryptString(It.IsAny<string>())).Returns((string s) => s);

            var command = CreateCommand(user.UserId);

            await _handler.Handle(command, CancellationToken.None);

            _roleCloneServiceMock.Verify(x => x.CloneRoleWithPrivilegesAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
