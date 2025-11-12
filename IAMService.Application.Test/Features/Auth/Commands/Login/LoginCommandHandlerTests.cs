using IAMService.Application.Features.Auth.Commands.Login;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Domain.Entities;
using Moq;
namespace IAMService.Application.Test.Features.Auth.Commands.Login
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {

        [SetUp]
        public void SetUp()
        {
            _authRepositoryMock = new Mock<IAuthRepository>();
            _userTokenGeneratorMock = new Mock<IUserTokenGenerator>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new LoginCommandHandler(
                _authRepositoryMock.Object,
                _userTokenGeneratorMock.Object,
                _unitOfWorkMock.Object
            );
        }
        private Mock<IAuthRepository> _authRepositoryMock;
        private Mock<IUserTokenGenerator> _userTokenGeneratorMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private LoginCommandHandler _handler;

        [Test]
        public async Task Handle_ShouldReturnLoginResponse_WhenLoginIsSuccessful()
        {
            // Arrange
            var command = new LoginCommand("test@example.com", "password123");
            var cancellationToken = CancellationToken.None;

            var role = new Domain.Entities.Role { RoleId = 1, RoleCode = "ADMIN" };
            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                FullName = "Test User",
                Email = "test@example.com",
                RoleId = 1,
                Role = role
            };

            var jwtToken = new JwtToken
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                UserId = user.UserId
            };

            _authRepositoryMock.Setup(x => x.Login(command.Email, command.Password, cancellationToken))
                .ReturnsAsync(user);
            _authRepositoryMock.Setup(x => x.DeleteAllJwtTokens(user.UserId, cancellationToken))
                .Returns(Task.FromResult(1));
            _userTokenGeneratorMock.Setup(x => x.GenerateTokenAsync(user, cancellationToken))
                .ReturnsAsync(jwtToken);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UserId, Is.EqualTo(user.UserId));
            Assert.That(result.FullName, Is.EqualTo("Test User"));
            Assert.That(result.Email, Is.EqualTo("test@example.com"));
            Assert.That(result.RoleCode, Is.EqualTo("ADMIN"));
            Assert.That(result.AccessToken, Is.EqualTo("access-token"));

            _authRepositoryMock.Verify(x => x.Login(command.Email, command.Password, cancellationToken), Times.Once);
            _authRepositoryMock.Verify(x => x.DeleteAllJwtTokens(user.UserId, cancellationToken), Times.Once);
            _userTokenGeneratorMock.Verify(x => x.GenerateTokenAsync(user, cancellationToken), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        }

        [Test]
        public void Handle_ShouldThrowUnauthorizedAccessException_WhenLoginFails()
        {
            // Arrange
            var command = new LoginCommand("wrong@example.com", "badpassword");
            var cancellationToken = CancellationToken.None;

            _authRepositoryMock.Setup(x => x.Login(command.Email, command.Password, cancellationToken))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, cancellationToken));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex!.Message, Is.EqualTo("Invalid credentials"));

            _authRepositoryMock.Verify(x => x.Login(command.Email, command.Password, cancellationToken), Times.Once);
            _authRepositoryMock.Verify(x => x.DeleteAllJwtTokens(It.IsAny<Guid>(), cancellationToken), Times.Never);
            _userTokenGeneratorMock.Verify(x => x.GenerateTokenAsync(It.IsAny<Domain.Entities.User>(), cancellationToken), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Never);
        }

        [Test]
        public async Task Handle_ShouldMapCorrectUserData_WhenUserHasDifferentRole()
        {
            // Arrange
            var command = new LoginCommand("staff@example.com", "password");
            var cancellationToken = CancellationToken.None;

            var role = new Domain.Entities.Role { RoleId = 2, RoleCode = "STAFF" };
            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                FullName = "Staff User",
                Email = "staff@example.com",
                Role = role
            };

            var jwtToken = new JwtToken
            {
                AccessToken = "staff-token",
                RefreshToken = "staff-refresh",
                UserId = user.UserId
            };

            _authRepositoryMock.Setup(x => x.Login(command.Email, command.Password, cancellationToken))
                .ReturnsAsync(user);
            _authRepositoryMock.Setup(x => x.DeleteAllJwtTokens(user.UserId, cancellationToken))
                .Returns(Task.FromResult(1));
            _userTokenGeneratorMock.Setup(x => x.GenerateTokenAsync(user, cancellationToken))
                .ReturnsAsync(jwtToken);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo("staff@example.com"));
            Assert.That(result.FullName, Is.EqualTo("Staff User"));
            Assert.That(result.RoleCode, Is.EqualTo("STAFF"));
            Assert.That(result.AccessToken, Is.EqualTo("staff-token"));
        }

        [Test]
        public async Task Handle_ShouldStillReturnResponse_WhenSaveChangesReturnsZero()
        {
            // Arrange
            var command = new LoginCommand("nosave@example.com", "password");
            var cancellationToken = CancellationToken.None;

            var role = new Domain.Entities.Role { RoleId = 3, RoleCode = "USER" };
            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                FullName = "No Save User",
                Email = "nosave@example.com",
                Role = role
            };

            var jwtToken = new JwtToken
            {
                AccessToken = "no-save-token",
                RefreshToken = "no-save-refresh",
                UserId = user.UserId
            };

            _authRepositoryMock.Setup(x => x.Login(command.Email, command.Password, cancellationToken))
                .ReturnsAsync(user);
            _authRepositoryMock.Setup(x => x.DeleteAllJwtTokens(user.UserId, cancellationToken))
                .Returns(Task.FromResult(1));
            _userTokenGeneratorMock.Setup(x => x.GenerateTokenAsync(user, cancellationToken))
                .ReturnsAsync(jwtToken);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(cancellationToken))
                .ReturnsAsync(0); // simulate no DB changes

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.EqualTo("no-save-token"));
            Assert.That(result.RoleCode, Is.EqualTo("USER"));
            Assert.That(result.Email, Is.EqualTo("nosave@example.com"));

            _authRepositoryMock.Verify(x => x.Login(command.Email, command.Password, cancellationToken), Times.Once);
            _authRepositoryMock.Verify(x => x.DeleteAllJwtTokens(user.UserId, cancellationToken), Times.Once);
            _userTokenGeneratorMock.Verify(x => x.GenerateTokenAsync(user, cancellationToken), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        }
    }
}
