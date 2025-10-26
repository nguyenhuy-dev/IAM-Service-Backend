using IAMService.Application.Features.Auth.Commands.RefreshToken;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Domain.Entities;
using Moq;

namespace IAMService.Application.Test.Features.Auth.Commands.RefreshToken
{
    [TestFixture]
    public class RefreshTokenCommandHandlerTests
    {
        private Mock<IAuthRepository> _authRepositoryMock = null!;
        private Mock<IUserTokenGenerator> _tokenGeneratorMock = null!;
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        private RefreshTokenCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _authRepositoryMock = new Mock<IAuthRepository>();
            _tokenGeneratorMock = new Mock<IUserTokenGenerator>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new RefreshTokenCommandHandler(
                _authRepositoryMock.Object,
                _tokenGeneratorMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Test]
        public async Task Handle_ShouldReturnNewToken_WhenRefreshSuccessful()
        {
            // Arrange
            var oldToken = "old_access_token";
            var command = new RefreshTokenCommand(oldToken);

            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                Email = "user@example.com",
                HashedPassword = "hash",
                Role = new Domain.Entities.Role
                {
                    RoleId = 1,
                    RoleCode = "ADMIN",
                    RoleName = "Administrator"
                }
            };

            var newJwtToken = new JwtToken
            {
                AccessToken = "new_access_token",
                RefreshToken = "refresh_token"
            };

            _authRepositoryMock
                .Setup(r => r.GetUserWithOldAccessToken(oldToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _authRepositoryMock
                .Setup(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            _tokenGeneratorMock
                .Setup(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newJwtToken);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.EqualTo("new_access_token"));

            _authRepositoryMock.Verify(r => r.GetUserWithOldAccessToken(oldToken, It.IsAny<CancellationToken>()), Times.Once);
            _authRepositoryMock.Verify(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()), Times.Once);
            _tokenGeneratorMock.Verify(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldThrowException_WhenDeleteFails()
        {
            // Arrange
            var oldToken = "old_access_token";
            var command = new RefreshTokenCommand(oldToken);

            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                Email = "fail@example.com",
                HashedPassword = "hash",
                Role = new Domain.Entities.Role
                {
                    RoleId = 2,
                    RoleCode = "USER",
                    RoleName = "User"
                }
            };

            _authRepositoryMock
                .Setup(r => r.GetUserWithOldAccessToken(oldToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _authRepositoryMock
                .Setup(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            Assert.That(ex!.Message, Is.EqualTo("Delete all related tokens unsuccessfully!"));

            _authRepositoryMock.Verify(r => r.GetUserWithOldAccessToken(oldToken, It.IsAny<CancellationToken>()), Times.Once);
            _authRepositoryMock.Verify(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()), Times.Once);
            _tokenGeneratorMock.Verify(t => t.GenerateTokenAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
