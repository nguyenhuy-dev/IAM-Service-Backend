using IAMService.Application.Features.Auth.Commands.Logout;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using Moq;

namespace IAMService.Application.Test.Features.Auth.Commands.Logout
{
    [TestFixture]
    public class LogoutCommandHandlerTests
    {
        private Mock<IAuthRepository> _authRepositoryMock = null!;
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        private LogoutCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _authRepositoryMock = new Mock<IAuthRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new LogoutCommandHandler(_authRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Test]
        public async Task Handle_Should_Return_True_When_Delete_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new LogoutCommand(userId);

            _authRepositoryMock
                .Setup(r => r.DeleteAllJwtTokens(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _authRepositoryMock.Verify(r => r.DeleteAllJwtTokens(userId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Handle_Should_ThrowException_When_Delete_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new LogoutCommand(userId);

            _authRepositoryMock
                .Setup(r => r.DeleteAllJwtTokens(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            Assert.That(ex!.Message, Is.EqualTo("Delete all related tokens unsuccessfully!"));
            _authRepositoryMock.Verify(r => r.DeleteAllJwtTokens(userId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
