using FluentAssertions;
using IAMService.Application.Features.ForgotPassword.Commands;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.ForgetPassword;
using IAMService.Domain.Entities;
using Moq;
using UserEntity=IAMService.Domain.Entities.User;

namespace IAMService.Application.Test.Features.Auth.Commands.ForgotPassword
{
    [TestFixture]
    public class ForgotPasswordCommandHandlerTests
    {

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _tokenHasherMock = new Mock<ITokenHasher>();
            _tokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _stringEncryptionServiceMock = new Mock<IStringEncryptionService>();

            _handler = new ForgotPasswordCommandHandler(
                _userRepositoryMock.Object,
                _emailServiceMock.Object,
                _tokenHasherMock.Object,
                _tokenRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _stringEncryptionServiceMock.Object
            );
        }
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<ITokenHasher> _tokenHasherMock;
        private Mock<IPasswordResetTokenRepository> _tokenRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IStringEncryptionService> _stringEncryptionServiceMock;
        private ForgotPasswordCommandHandler _handler;

        [Test]
        public async Task Handle_ValidEmail_ShouldReturnTrueAndSendEmail()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "test@example.com" };
            var cancellationToken = CancellationToken.None;
            var userId = Guid.NewGuid();
            var encryptedEmail = "encrypted_test@example.com";
            var decryptedEmail = "test@example.com";
            var hashedToken = "hashed-token";

            var user = new UserEntity
            {
                UserId = userId,
                Email = encryptedEmail,
                FullName = "Test User"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _stringEncryptionServiceMock.Setup(x => x.DecryptString(encryptedEmail))
                .Returns(decryptedEmail);
            _tokenHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns(hashedToken);
            _tokenRepositoryMock.Setup(x => x.DeleteExistingTokensByUserIdAsync(userId))
                .Returns(Task.CompletedTask);
            _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordResetToken>()))
                .ReturnsAsync(new PasswordResetToken());
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _emailServiceMock.Setup(x => x.SendPasswordResetEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())) // ← THÊM PARAMETER NÀY
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
            _stringEncryptionServiceMock.Verify(x => x.DecryptString(encryptedEmail), Times.Once);
            _tokenRepositoryMock.Verify(x => x.DeleteExistingTokensByUserIdAsync(userId), Times.Once);
            _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once); // ← THÊM PARAMETER NÀY
        }

        [Test]
        public async Task Handle_NonExistentEmail_ShouldReturnTrueWithoutSendingEmail()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "nonexistent@example.com" };
            var cancellationToken = CancellationToken.None;

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync((UserEntity)null!);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never); // ← THÊM PARAMETER NÀY
            _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_LockedOutUser_ShouldReturnTrueWithoutSendingEmail()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "locked@example.com" };
            var cancellationToken = CancellationToken.None;

            var user = new UserEntity
            {
                UserId = Guid.NewGuid(),
                Email = "encrypted_locked@example.com",
                FullName = "Locked User"
            };
            user.LockAccount(DateTimeOffset.UtcNow.AddHours(1));

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(command.Email), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never); // ← THÊM PARAMETER NÀY
            _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_UserWithEmptyEmail_ShouldReturnTrueWithoutSendingEmail()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "test@example.com" };
            var cancellationToken = CancellationToken.None;

            var user = new UserEntity
            {
                UserId = Guid.NewGuid(),
                Email = string.Empty,
                FullName = "Test User"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();
            _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never); // ← THÊM PARAMETER NÀY
        }

        [Test]
        public async Task Handle_InvalidDecryptedEmail_ShouldReturnTrueWithoutSendingEmail()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "test@example.com" };
            var cancellationToken = CancellationToken.None;
            var invalidDecryptedEmail = "not-an-email";

            var user = new UserEntity
            {
                UserId = Guid.NewGuid(),
                Email = "encrypted_email",
                FullName = "Test User"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _stringEncryptionServiceMock.Setup(x => x.DecryptString(user.Email))
                .Returns(invalidDecryptedEmail);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();
            _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never); // ← THÊM PARAMETER NÀY
        }

        [Test]
        public async Task Handle_ValidRequest_ShouldDeleteExistingTokensBeforeCreatingNew()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "test@example.com" };
            var cancellationToken = CancellationToken.None;
            var userId = Guid.NewGuid();

            var user = new UserEntity
            {
                UserId = userId,
                Email = "encrypted_test@example.com",
                FullName = "Test User"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _stringEncryptionServiceMock.Setup(x => x.DecryptString(user.Email))
                .Returns("test@example.com");
            _tokenHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("hashed");
            _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordResetToken>()))
                .ReturnsAsync(new PasswordResetToken());
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _emailServiceMock.Setup(x => x.SendPasswordResetEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())) // ← THÊM PARAMETER NÀY
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _tokenRepositoryMock.Verify(x => x.DeleteExistingTokensByUserIdAsync(userId), Times.Once);
        }

        [Test]
        public async Task Handle_ValidRequest_ShouldCreateTokenAndSendEmail()
        {
            // Arrange
            var command = new ForgotPasswordCommand { Email = "test@example.com" };
            var cancellationToken = CancellationToken.None;

            var user = new UserEntity
            {
                UserId = Guid.NewGuid(),
                Email = "encrypted_test@example.com",
                FullName = "Test User"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _stringEncryptionServiceMock.Setup(x => x.DecryptString(user.Email))
                .Returns("test@example.com");
            _tokenHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("hashed");
            _tokenRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PasswordResetToken>()))
                .ReturnsAsync(new PasswordResetToken());
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _emailServiceMock.Setup(x => x.SendPasswordResetEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())) // ← THÊM PARAMETER NÀY
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _tokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Once); // ← THÊM PARAMETER NÀY
        }
    }
}
