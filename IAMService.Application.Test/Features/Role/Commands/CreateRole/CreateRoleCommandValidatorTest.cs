using FluentValidation.TestHelper;
using IAMService.Application.Features.Role.Commands.CreateRole;
using IAMService.Application.Interfaces;
using NSubstitute;
namespace IAMService.Application.Test.Features.Role.Commands.CreateRole
{
    /// <summary>
    /// Unit tests for CreateRoleCommandValidator
    /// Tests validation rules for role creation including:
    /// - Role name uniqueness and format
    /// - Role code uniqueness
    /// - Privilege IDs validation
    /// - Description length constraints
    /// </summary>
    [TestFixture]
    public class CreateRoleCommandValidatorTest
    {
        private IRoleRepository _roleRepository;
        private IPrivilegeRepository _privilegeRepository;
        private CreateRoleCommandValidator _validator;

        /// <summary>
        /// Initialize test dependencies before each test
        /// Creates mock repositories and validator instance
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _privilegeRepository = Substitute.For<IPrivilegeRepository>();
            _validator = new CreateRoleCommandValidator(_roleRepository, _privilegeRepository);
        }

        /// <summary>
        /// Test: Valid command should pass all validation rules
        /// Verifies that a properly formed command with valid data passes validation
        /// </summary>
        [Test]
        public async Task Should_Not_Have_Error_When_Command_Is_Valid()
        {
            // Arrange: Create valid command with unique name and code
            var command = new CreateRoleCommand(
                "Admin",
                "ADMIN_CODE",
                "This is a valid description.",
                new List<int> { 1, 2 }
            );

            // Mock repository responses for uniqueness checks
            _roleRepository.ExistsByNameAsync(command.RoleName).Returns(false);
            _roleRepository.ExistsByCodeAsync(command.RoleCode).Returns(false);
            _privilegeRepository.AllExistAsync(Arg.Is<List<int>>(p => p.SequenceEqual(command.PrivilegeIds)))
                              .Returns(true);

            // Act: Validate the command
            var result = await _validator.TestValidateAsync(command);

            // Assert: No validation errors should occur
            result.ShouldNotHaveAnyValidationErrors();
        }
        
        /// <summary>
        /// Test: Empty or whitespace role name should trigger validation error
        /// Business Rule: Role name is mandatory and cannot be empty
        /// </summary>
        [TestCase("")]
        [TestCase("   ")]
        public async Task Should_Have_Error_When_RoleName_Is_Empty(string roleName)
        {
            // Arrange: Create command with invalid role name
            var command = new CreateRoleCommand(roleName, "CODE", "Desc", new List<int> { 1, 2 });
            
            // Act: Validate the command
            var result = await _validator.TestValidateAsync(command);
            
            // Assert: Should have specific error message for RoleName
            result.ShouldHaveValidationErrorFor(c => c.RoleName)
                  .WithErrorMessage("RoleName is required.");
        }

        /// <summary>
        /// Test: Duplicate role name should be rejected
        /// Business Rule: Role names must be unique across the system
        /// </summary>
        [Test]
        public async Task Should_Have_Error_When_RoleName_Already_Exists()
        {
            // Arrange: Create command with existing role name
            var command = new CreateRoleCommand("Existing Role", "CODE", "Desc", new List<int> { 1, 2 });
            _roleRepository.ExistsByNameAsync(command.RoleName).Returns(true);

            // Act: Validate the command
            var result = await _validator.TestValidateAsync(command);
            
            // Assert: Should reject duplicate role name with specific error
            result.ShouldHaveValidationErrorFor(c => c.RoleName)
                  .WithErrorMessage("Role with name 'Existing Role' already exists.");
        }

        /// <summary>
        /// Test: Duplicate role code should be rejected
        /// Business Rule: Role codes must be unique across the system
        /// </summary>
        [Test]
        public async Task Should_Have_Error_When_RoleCode_Already_Exists()
        {
            // Arrange: Create command with existing role code
            var command = new CreateRoleCommand("Role", "EXISTING_CODE", "Desc", new List<int> { 1, 2 });
            _roleRepository.ExistsByCodeAsync(command.RoleCode).Returns(true);

            // Act: Validate the command
            var result = await _validator.TestValidateAsync(command);
            
            // Assert: Should reject duplicate role code with specific error
            result.ShouldHaveValidationErrorFor(c => c.RoleCode)
                  .WithErrorMessage("Role with code 'EXISTING_CODE' already exists.");
        }

        /// <summary>
        /// Test: Non-existent privilege IDs should be rejected
        /// Business Rule: All privilege IDs must exist in the database before assignment
        /// </summary>
        [Test]
        public async Task Should_Have_Error_When_PrivilegeIds_Are_Invalid()
        {
            // Arrange: Create command with invalid privilege ID (999)
            var command = new CreateRoleCommand("Role", "CODE", "Desc", new List<int> { 1, 999 });
            _privilegeRepository.AllExistAsync(command.PrivilegeIds).Returns(false);
            
            // Act: Validate the command
            var result = await _validator.TestValidateAsync(command);
            
            // Assert: Should reject invalid privilege IDs
            result.ShouldHaveValidationErrorFor(c => c.PrivilegeIds)
                  .WithErrorMessage("One or more privilege IDs are invalid.");
        }
        
        /// <summary>
        /// Test: Empty privilege list should be allowed
        /// Business Rule: Roles can be created without privileges initially
        /// </summary>
        [Test]
        public async Task Should_Not_Have_Error_For_Privileges_When_PrivilegeIds_List_Is_Empty()
        {
            // Arrange: Create command with empty privilege list
            var command = new CreateRoleCommand("Role", "CODE", "Desc", new List<int>());
            
            // Act: Validate the command
            var result = await _validator.TestValidateAsync(command);
            
            // Assert: Empty privilege list should be valid
            result.ShouldNotHaveValidationErrorFor(c => c.PrivilegeIds);
        }

        /// <summary>
        /// Test: Description exceeding maximum length should be rejected
        /// Business Rule: Description is limited to 200 characters
        /// </summary>
        [Test]
        public async Task Should_Have_Error_When_Description_Exceeds_Max_Length()
        {
            // Arrange: Create command with description over 200 characters
            var longDescription = new string('a', 201);
            var command = new CreateRoleCommand("Role", "CODE", longDescription, new List<int> { 1, 2 });
            
            // Act: Validate the command
            var result = await _validator.TestValidateAsync(command);
            
            // Assert: Should reject description exceeding max length
            result.ShouldHaveValidationErrorFor(c => c.Description)
                  .WithErrorMessage("Description must not exceed 200 characters.");
        }
    }
}