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
        private Mock<IAuthRepository> _authRepositoryMock = null!;
        private Mock<IUserTokenGenerator> _tokenGeneratorMock = null!;
        private Mock<IUnitOfWork> _unitOfWorkMock = null!;
        private LoginCommandHandler _handler = null!;

        [SetUp]
        public void Setup()
        {
            _authRepositoryMock = new Mock<IAuthRepository>();
            _tokenGeneratorMock = new Mock<IUserTokenGenerator>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new LoginCommandHandler(
                _authRepositoryMock.Object,
                _tokenGeneratorMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Test]
        public async Task Handle_ShouldReturnLoginResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var command = new LoginCommand("user@example.com", "P@ssword123");

            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                HashedPassword = "hashed",
                Role = new Domain.Entities.Role
                {
                    RoleId = 1,
                    RoleCode = "ADMIN",
                    RoleName = "Administrator",
                    Description = "System admin"
                }
            };

            var jwtToken = new JwtToken
            {
                Id = Guid.NewGuid(),
                AccessToken = "mock_access_token",
                RefreshToken = "mock_refresh_token",
                CreateAt = DateTime.UtcNow,
                UserId = user.UserId,
                User = user
            };

            _authRepositoryMock
                .Setup(r => r.Login(command.Email, command.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _authRepositoryMock
                .Setup(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _tokenGeneratorMock
                .Setup(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jwtToken);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(command.Email));
            Assert.That(result.RoleCode, Is.EqualTo("ADMIN"));
            Assert.That(result.AccessToken, Is.EqualTo("mock_access_token"));

            _authRepositoryMock.Verify(r => r.Login(command.Email, command.Password, It.IsAny<CancellationToken>()), Times.Once);
            _authRepositoryMock.Verify(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()), Times.Once);
            _tokenGeneratorMock.Verify(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Handle_ShouldThrowException_WhenLoginFails()
        {
            // Arrange
            var command = new LoginCommand("wrong@example.com", "WrongPassword");

            _authRepositoryMock
                .Setup(r => r.Login(command.Email, command.Password, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act + Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            _authRepositoryMock.Verify(r => r.Login(command.Email, command.Password, It.IsAny<CancellationToken>()), Times.Once);
            _authRepositoryMock.Verify(r => r.DeleteAllJwtTokens(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _tokenGeneratorMock.Verify(t => t.GenerateTokenAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ShouldReturnResponse_WhenDeleteAllJwtTokensReturnsZero()
        {
            // Arrange
            var command = new LoginCommand("user2@example.com", "Password123");

            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                HashedPassword = "hashed",
                Role = new Domain.Entities.Role
                {
                    RoleId = 2,
                    RoleCode = "USER",
                    RoleName = "Regular User",
                    Description = "Standard access"
                }
            };

            var jwtToken = new JwtToken
            {
                Id = Guid.NewGuid(),
                AccessToken = "access_token_2",
                RefreshToken = "refresh_token_2",
                CreateAt = DateTime.UtcNow,
                UserId = user.UserId,
                User = user
            };

            _authRepositoryMock
                .Setup(r => r.Login(command.Email, command.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _authRepositoryMock
                .Setup(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            _tokenGeneratorMock
                .Setup(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jwtToken);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RoleCode, Is.EqualTo("USER"));
            Assert.That(result.AccessToken, Is.EqualTo("access_token_2"));
        }

        [Test]
        public void Handle_ShouldThrowException_WhenUnitOfWorkFails()
        {
            // Arrange
            var command = new LoginCommand("error@example.com", "Password123");

            var user = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                HashedPassword = "hashed",
                Role = new Domain.Entities.Role
                {
                    RoleId = 3,
                    RoleCode = "STAFF",
                    RoleName = "Staff Member",
                    Description = "General staff"
                }
            };

            var jwtToken = new JwtToken
            {
                Id = Guid.NewGuid(),
                AccessToken = "error_token",
                RefreshToken = "refresh_token",
                CreateAt = DateTime.UtcNow,
                UserId = user.UserId,
                User = user
            };

            _authRepositoryMock
                .Setup(r => r.Login(command.Email, command.Password, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _authRepositoryMock
                .Setup(r => r.DeleteAllJwtTokens(user.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _tokenGeneratorMock
                .Setup(t => t.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jwtToken);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act + Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            Assert.That(ex!.Message, Is.EqualTo("Database error"));
        }
    }
}
