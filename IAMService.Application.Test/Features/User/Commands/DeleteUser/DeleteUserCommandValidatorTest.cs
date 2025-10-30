using FluentAssertions;
using IAMService.Application.Features.User.Commands.DeleteUser;
using IAMService.Application.Interfaces;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace IAMService.Application.Test.Features.User.Commands.DeleteUser
{
    /// <summary>
    /// Unit tests for <see cref="DeleteUserCommandValidator"/>
    /// </summary>
    [TestFixture]
    public class DeleteUserCommandValidatorTests
    {
        private IUserRepository _userRepository;
        private DeleteUserCommandValidator _validator;

        [SetUp]
        public void Setup()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _validator = new DeleteUserCommandValidator(_userRepository);
        }

        [Test]
        public async Task Validate_ValidUserId_ShouldPass()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteUserCommand(userId);
            _userRepository.GetByIdAsync(userId).Returns(new Domain.Entities.User { UserId = userId });

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public async Task Validate_EmptyUserId_ShouldFail()
        {
            // Arrange
            var command = new DeleteUserCommand(Guid.Empty);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("UserId is required.");
        }

        [Test]
        public async Task Validate_UserNotFound_ShouldFail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteUserCommand(userId);
            _userRepository.GetByIdAsync(userId).ReturnsNull();

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("User not found.");
        }

        [Test]
        public async Task Validate_WithCancellationToken_PassesToken()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteUserCommand(userId);
            var token = new CancellationToken();
            _userRepository.GetByIdAsync(userId).Returns(new Domain.Entities.User { UserId = userId });

            // Act
            await _validator.ValidateAsync(command, token);

            // Assert
            await _userRepository.Received(1).GetByIdAsync(userId);
        }
    }
}
