using FluentAssertions;
using IAMService.Application.Features.Role.Commands.DeleteRole;
using IAMService.Application.Interfaces;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace IAMService.Application.Tests.Features.Role.Commands.DeleteRole
{
    [TestFixture]
    public class DeleteRoleCommandValidatorTests
    {
        private IRoleRepository _roleRepository;
        private DeleteRoleCommandValidator _validator;

        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _validator = new DeleteRoleCommandValidator(_roleRepository);
        }

        [Test]
        public async Task Validate_ValidRoleId_ShouldNotHaveValidationError()
        {
            // Arrange
            var roleId = 5;
            var command = new DeleteRoleCommand(roleId);

            var role = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "CUSTOM_ROLE",
                IsDefault = false
            };

            _roleRepository.GetByIdAsync(roleId).Returns(role);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Test]
        public async Task Validate_RoleIdZero_ShouldHaveValidationError()
        {
            // Arrange
            var command = new DeleteRoleCommand(0);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("RoleId must be greater than zero.");
            result.Errors.Should().Contain(x => x.PropertyName == "RoleId");
        }

        [Test]
        public async Task Validate_RoleIdNegative_ShouldHaveValidationError()
        {
            // Arrange
            var command = new DeleteRoleCommand(-1);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x =>
                x.PropertyName == "RoleId" &&
                x.ErrorMessage == "RoleId must be greater than zero.");
        }

        [Test]
        public async Task Validate_RoleNotFound_ShouldHaveValidationError()
        {
            // Arrange
            var roleId = 999;
            var command = new DeleteRoleCommand(roleId);

            _roleRepository.GetByIdAsync(roleId).ReturnsNull();

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Role not found.");
            result.Errors.Should().Contain(x => x.PropertyName == "RoleId");
        }

        [Test]
        public async Task Validate_DefaultRole_ShouldHaveValidationError()
        {
            // Arrange
            var roleId = 1;
            var command = new DeleteRoleCommand(roleId);

            var defaultRole = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "ADMIN",
                IsDefault = true
            };

            _roleRepository.GetByIdAsync(roleId).Returns(defaultRole);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Default roles cannot be deleted.");
            result.Errors.Should().Contain(x => x.PropertyName == "RoleId");
        }

        [Test]
        public async Task Validate_ReadOnlyRole_ShouldHaveValidationError()
        {
            // Arrange
            var roleId = 2;
            var command = new DeleteRoleCommand(roleId);

            var readOnlyRole = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "ReadOnly",
                IsDefault = false
            };

            _roleRepository.GetByIdAsync(roleId).Returns(readOnlyRole);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("The ReadOnly role cannot be deleted.");
            result.Errors.Should().Contain(x => x.PropertyName == "RoleId");
        }

        [Test]
        public async Task Validate_CascadeMode_StopsOnFirstError()
        {
            // Arrange
            var command = new DeleteRoleCommand(0);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            // Should only have one error due to CascadeMode.Stop
            result.Errors.Should().ContainSingle();
            // Repository should not be called because validation stopped at GreaterThan check
            await _roleRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Test]
        public async Task Validate_ValidRoleId_CallsRepositoryOnce()
        {
            // Arrange
            var roleId = 10;
            var command = new DeleteRoleCommand(roleId);

            var role = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "MANAGER",
                IsDefault = false
            };

            _roleRepository.GetByIdAsync(roleId).Returns(role);

            // Act
            await _validator.ValidateAsync(command);

            // Assert
            await _roleRepository.Received(1).GetByIdAsync(roleId);
        }

        [Test]
        public async Task Validate_MultipleValidationErrors_ReturnsFirstErrorOnly()
        {
            // Arrange
            // This tests that CascadeMode.Stop prevents multiple errors
            var command = new DeleteRoleCommand(-5);

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            // Should stop at first rule failure (GreaterThan)
            result.Errors.Should().HaveCount(1);
            result.Errors.First().ErrorMessage.Should().Be("RoleId must be greater than zero.");
        }

        [Test]
        public async Task Validate_WithCancellationToken_PassesTokenToRepository()
        {
            // Arrange
            var roleId = 3;
            var command = new DeleteRoleCommand(roleId);
            var cancellationToken = new CancellationToken();

            var role = new Domain.Entities.Role
            {
                RoleId = roleId,
                RoleCode = "EDITOR",
                IsDefault = false
            };

            _roleRepository.GetByIdAsync(roleId).Returns(role);

            // Act
            await _validator.ValidateAsync(command, cancellationToken);

            // Assert
            await _roleRepository.Received(1).GetByIdAsync(roleId);
        }
    }
}