using FluentAssertions;
using IAMService.Application.Features.ResetPassword.Commands;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.ForgetPassword;
using IAMService.Domain.Entities;
using Moq;
using UserEntity=IAMService.Domain.Entities.User;

namespace IAMService.Application.Test.Features.Auth.Commands.ResetPassword
{
    [TestFixture]
    public class ResetPasswordCommandHandlerTests
    {

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenHasherMock = new Mock<ITokenHasher>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new ResetPasswordCommandHandler(
                _userRepositoryMock.Object,
                _tokenHasherMock.Object,
                _passwordHasherMock.Object,
                _tokenRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ITokenHasher> _tokenHasherMock;
        private Mock<IPasswordHasher> _passwordHasherMock;
        private Mock<IPasswordResetTokenRepository> _tokenRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private ResetPasswordCommandHandler _handler;

        [Test]
        public async Task Handle_ValidRequest_ShouldResetPasswordAndReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var rawToken = "raw-reset-token";
            var hashedToken = "hashed-reset-token";
            var newPassword = "NewP@ssw0rd!";
            var hashedPassword = "hashed-new-password";

            var command = new ResetPasswordCommand
            {
                UserId = userId,
                Token = rawToken,
                NewPassword = newPassword
            };

            var tokenEntity = new PasswordResetToken
            {
                UserId = userId,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            var user = new UserEntity
            {
                UserId = userId,
                Email = "test@example.com",
                FullName = "Test User",
                HashedPassword = "old-hashed-password"
            };

            _tokenHasherMock.Setup(x => x.Hash(rawToken))
                .Returns(hashedToken);
            _tokenRepositoryMock.Setup(x => x.GetValidTokenByHashedTokenAsync(hashedToken))
                .ReturnsAsync(tokenEntity);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.HashPassword(newPassword))
                .Returns(hashedPassword);
            _tokenRepositoryMock.Setup(x => x.MarkAsUsed(tokenEntity))
                .Verifiable();
            _userRepositoryMock.Setup(x => x.UpdateAsync(user))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            user.HashedPassword.Should().Be(hashedPassword);
            _tokenHasherMock.Verify(x => x.Hash(rawToken), Times.Once);
            _tokenRepositoryMock.Verify(x => x.GetValidTokenByHashedTokenAsync(hashedToken), Times.Once);
            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId, true), Times.Once);
            _passwordHasherMock.Verify(x => x.HashPassword(newPassword), Times.Once);
            _tokenRepositoryMock.Verify(x => x.MarkAsUsed(tokenEntity), Times.Once);
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_InvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var rawToken = "invalid-token";
            var hashedToken = "hashed-invalid-token";

            var command = new ResetPasswordCommand
            {
                UserId = userId,
                Token = rawToken,
                NewPassword = "NewP@ssw0rd!"
            };

            _tokenHasherMock.Setup(x => x.Hash(rawToken))
                .Returns(hashedToken);
            _tokenRepositoryMock.Setup(x => x.GetValidTokenByHashedTokenAsync(hashedToken))
                .ReturnsAsync((PasswordResetToken)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_TokenUserIdMismatch_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var rawToken = "valid-token";
            var hashedToken = "hashed-token";

            var command = new ResetPasswordCommand
            {
                UserId = userId,
                Token = rawToken,
                NewPassword = "NewP@ssw0rd!"
            };

            var tokenEntity = new PasswordResetToken
            {
                UserId = differentUserId,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _tokenHasherMock.Setup(x => x.Hash(rawToken))
                .Returns(hashedToken);
            _tokenRepositoryMock.Setup(x => x.GetValidTokenByHashedTokenAsync(hashedToken))
                .ReturnsAsync(tokenEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Handle_UserNotFound_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var rawToken = "valid-token";
            var hashedToken = "hashed-token";

            var command = new ResetPasswordCommand
            {
                UserId = userId,
                Token = rawToken,
                NewPassword = "NewP@ssw0rd!"
            };

            var tokenEntity = new PasswordResetToken
            {
                UserId = userId,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _tokenHasherMock.Setup(x => x.Hash(rawToken))
                .Returns(hashedToken);
            _tokenRepositoryMock.Setup(x => x.GetValidTokenByHashedTokenAsync(hashedToken))
                .ReturnsAsync(tokenEntity);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync((UserEntity)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
            _tokenRepositoryMock.Verify(x => x.MarkAsUsed(It.IsAny<PasswordResetToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_LockedOutUser_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var rawToken = "valid-token";
            var hashedToken = "hashed-token";

            var command = new ResetPasswordCommand
            {
                UserId = userId,
                Token = rawToken,
                NewPassword = "NewP@ssw0rd!"
            };

            var tokenEntity = new PasswordResetToken
            {
                UserId = userId,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            var user = new UserEntity
            {
                UserId = userId,
                Email = "locked@example.com",
                FullName = "Locked User"
            };
            user.LockAccount(DateTimeOffset.UtcNow.AddHours(1));

            _tokenHasherMock.Setup(x => x.Hash(rawToken))
                .Returns(hashedToken);
            _tokenRepositoryMock.Setup(x => x.GetValidTokenByHashedTokenAsync(hashedToken))
                .ReturnsAsync(tokenEntity);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
            _tokenRepositoryMock.Verify(x => x.MarkAsUsed(It.IsAny<PasswordResetToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ValidRequest_ShouldMarkTokenAsUsed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var rawToken = "valid-token";
            var hashedToken = "hashed-token";

            var command = new ResetPasswordCommand
            {
                UserId = userId,
                Token = rawToken,
                NewPassword = "NewP@ssw0rd!"
            };

            var tokenEntity = new PasswordResetToken
            {
                UserId = userId,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            var user = new UserEntity
            {
                UserId = userId,
                Email = "test@example.com",
                FullName = "Test User",
                HashedPassword = "old-password"
            };

            _tokenHasherMock.Setup(x => x.Hash(rawToken))
                .Returns(hashedToken);
            _tokenRepositoryMock.Setup(x => x.GetValidTokenByHashedTokenAsync(hashedToken))
                .ReturnsAsync(tokenEntity);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("new-hashed");
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tokenRepositoryMock.Verify(x => x.MarkAsUsed(tokenEntity), Times.Once);
        }

        [Test]
        public async Task Handle_ValidRequest_ShouldUpdateUserPassword()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var rawToken = "valid-token";
            var newPassword = "NewP@ssw0rd!";
            var hashedNewPassword = "hashed-new-password";

            var command = new ResetPasswordCommand
            {
                UserId = userId,
                Token = rawToken,
                NewPassword = newPassword
            };

            var tokenEntity = new PasswordResetToken
            {
                UserId = userId,
                Token = "hashed-token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            var user = new UserEntity
            {
                UserId = userId,
                Email = "test@example.com",
                FullName = "Test User",
                HashedPassword = "old-password"
            };

            _tokenHasherMock.Setup(x => x.Hash(rawToken))
                .Returns("hashed-token");
            _tokenRepositoryMock.Setup(x => x.GetValidTokenByHashedTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(tokenEntity);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.HashPassword(newPassword))
                .Returns(hashedNewPassword);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.HashedPassword.Should().Be(hashedNewPassword);
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }
    }
}
