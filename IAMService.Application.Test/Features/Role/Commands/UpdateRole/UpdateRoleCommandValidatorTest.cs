using FluentValidation.TestHelper;
using IAMService.Application.Features.Role.Commands.UpdateRole;
using IAMService.Application.Interfaces;
using NSubstitute;
namespace IAMService.Application.Test.Features.Role.Commands.UpdateRole
{
    /// <summary>
    ///     The update role command validator test class
    /// </summary>
    [TestFixture]
    public class UpdateRoleCommandValidatorTest
    {

        /// <summary>
        ///     Setup this instance
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _privilegeRepository = Substitute.For<IPrivilegeRepository>();
            _validator = new UpdateRoleCommandValidator(_roleRepository, _privilegeRepository);
        }
        /// <summary>
        ///     The role repository
        /// </summary>
        private IRoleRepository _roleRepository;
        /// <summary>
        ///     The privilege repository
        /// </summary>
        private IPrivilegeRepository _privilegeRepository;
        /// <summary>
        ///     The validator
        /// </summary>
        private UpdateRoleCommandValidator _validator;

        /// <summary>
        ///     Tests that should not have error when command is valid
        /// </summary>
        [Test]
        public async Task Should_Not_Have_Error_When_Command_Is_Valid()
        {
            // ## Arrange ##
            var command = new UpdateRoleCommand(1, "New Name", "NEW_CODE", "Desc", new List<int> { 1, 2 });
            var existingRole = new Domain.Entities.Role(1, "Old Name", "OLD_CODE", "Old Desc");

            _roleRepository.GetByIdAsync(command.RoleId).Returns(existingRole);
            _roleRepository.ExistsByNameAsync(command.RoleName).Returns(false);
            _roleRepository.ExistsByCodeAsync(command.RoleCode).Returns(false);

            // ⬇️ *** FIX #1: Added missing mock setup for privileges *** ⬇️
            _privilegeRepository.AllExistAsync(Arg.Is<List<int>>(p => p.SequenceEqual(command.PrivilegeIds)))
                .Returns(true);

            // ## Act ##
            var result = await _validator.TestValidateAsync(command);

            // ## Assert ##
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        ///     Tests that should have error when role id not found
        /// </summary>
        [Test]
        public async Task Should_Have_Error_When_RoleId_Not_Found()
        {
            var command = new UpdateRoleCommand(999, "Name", "CODE", "Desc", [1, 2]);
            _roleRepository.GetByIdAsync(command.RoleId).Returns((Domain.Entities.Role)null);

            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.RoleId)
                .WithErrorMessage("Role not found.");
        }

        /// <summary>
        ///     Tests that should have error when updating a default role
        /// </summary>
        [Test]
        public async Task Should_Have_Error_When_Updating_A_Default_Role()
        {
            var command = new UpdateRoleCommand(1, "Name", "CODE", "Desc", [1, 2]);
            var defaultRole = new Domain.Entities.Role { RoleId = 1, IsDefault = true };
            _roleRepository.GetByIdAsync(command.RoleId).Returns(defaultRole);

            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.RoleId)
                .WithErrorMessage("Default roles cannot be updated.");
        }

        /// <summary>
        ///     Tests that should have error when updating the read only role
        /// </summary>
        [Test]
        public async Task Should_Have_Error_When_Updating_The_ReadOnly_Role()
        {
            var command = new UpdateRoleCommand(1, "Name", "CODE", "Desc", [1, 2]);
            var readOnlyRole = new Domain.Entities.Role { RoleId = 1, RoleCode = "ReadOnly" };
            _roleRepository.GetByIdAsync(command.RoleId).Returns(readOnlyRole);

            var result = await _validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(c => c.RoleId)
                .WithErrorMessage("The ReadOnly role cannot be updated.");
        }
    }
}
