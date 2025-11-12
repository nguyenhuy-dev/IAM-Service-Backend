using FluentAssertions;
using IAMService.Application.Exceptions;
using IAMService.Application.Features.User.Commands.DeleteUser;
using IAMService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
namespace IAMService.Application.Test.Features.User.Commands.DeleteUser
{
    /// <summary>
    ///     Unit tests for <see cref="DeleteUserCommandHandler" />
    /// </summary>
    [TestFixture]
    public class DeleteUserCommandHandlerTests
    {

        [SetUp]
        public void Setup()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _logger = Substitute.For<ILogger<DeleteUserCommandHandler>>();

            _handler = new DeleteUserCommandHandler(_userRepository, _unitOfWork, _logger);
        }
        private IUserRepository _userRepository;
        private IUnitOfWork _unitOfWork;
        private ILogger<DeleteUserCommandHandler> _logger;
        private DeleteUserCommandHandler _handler;

        [Test]
        public async Task Handle_ValidUser_DeletesUserSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteUserCommand(userId);

            var user = new Domain.Entities.User { UserId = userId, FullName = "John Doe" };
            _userRepository.GetByIdAsync(userId).Returns(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _userRepository.Received(1).Delete(user);
            await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
            _logger.ReceivedWithAnyArgs(1).LogInformation(default!);

        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteUserCommand(userId);
            _userRepository.GetByIdAsync(userId).ReturnsNull();

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*UserId*");
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesTokenToSaveChanges()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteUserCommand(userId);
            var cancellationToken = new CancellationToken(true);

            var user = new Domain.Entities.User { UserId = userId };
            _userRepository.GetByIdAsync(userId).Returns(user);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }
    }
}
