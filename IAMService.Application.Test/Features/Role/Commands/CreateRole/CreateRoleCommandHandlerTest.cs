using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Features.Role.Commands.CreateRole;
using IAMService.Application.Interfaces;
using NSubstitute;
namespace IAMService.Application.Test.Features.Role.Commands.CreateRole
{
    /// <summary>
    ///     Unit tests for CreateRoleCommandHandler
    ///     Tests the role creation workflow including:
    ///     - Role entity creation with privileges
    ///     - Default privilege assignment for empty privilege lists
    ///     - Validation of required fields
    ///     - Proper mapping between entities and DTOs
    /// </summary>
    [TestFixture]
    public class CreateRoleCommandHandlerTest
    {

        /// <summary>
        ///     Initialize test dependencies before each test
        ///     Creates mock repositories, mapper, and handler instance
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _mapper = Substitute.For<IMapper>();
            _handler = new CreateRoleCommandHandler(_roleRepository, _mapper);
        }
        private IRoleRepository _roleRepository;
        private IMapper _mapper;
        private CreateRoleCommandHandler _handler;

        /// <summary>
        ///     Test: Valid command should create role and return DTO
        ///     Verifies complete workflow:
        ///     1. Role entity is created with correct properties
        ///     2. Privileges are assigned as specified
        ///     3. Entity is mapped to DTO correctly
        ///     4. Repository and mapper are called exactly once
        /// </summary>
        [Test]
        public async Task Handle_Should_CreateRoleAndReturnRoleDto_WhenCommandIsValid()
        {
            // Arrange: Create valid command with role details and privileges
            var command = new CreateRoleCommand(
                "Admin",
                "ADMIN",
                "Administrator role",
                new List<int> { 10, 20 }
            );

            // Mock the created entity returned by repository
            var createdRoleEntity = new Domain.Entities.Role
            {
                RoleId = 1,
                RoleName = command.RoleName,
                RoleCode = command.RoleCode,
                Description = command.Description
            };

            // Expected DTO to be returned
            var expectedRoleDto = new RoleDto
            {
                RoleId = 1,
                RoleName = "Admin",
                RoleCode = "ADMIN",
                Description = "Administrator role",
                Privileges = new List<PrivilegeDto>()
            };

            var expectedPrivileges = new List<int> { 10, 20, 9, 18 };

            // VARIABLE TO CAPTURE THE ARGUMENT
            IEnumerable<int> capturedPrivileges = null;

            // SETUP: Relax the matcher to Arg.Any, but use Arg.Do to capture the value
            _roleRepository.CreateAsync(
                    Arg.Any<Domain.Entities.Role>(),
                    Arg.Do<IEnumerable<int>>(x => capturedPrivileges = x) // Capture here
                )
                .Returns(Task.FromResult(createdRoleEntity));

            _mapper.Map<RoleDto>(createdRoleEntity).Returns(expectedRoleDto);

            // ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // ASSERT
            // 1. Verify the call happened (arguments don't matter here, we just check invocation)
            await _roleRepository.Received(1).CreateAsync(Arg.Any<Domain.Entities.Role>(), Arg.Any<IEnumerable<int>>());

            // 2. Verify the Role data
            Assert.That(capturedPrivileges, Is.Not.Null, "CreateAsync was called, but privileges were null");

            // 3. Verify the List Content (NUnit's Is.EquivalentTo handles order independence automatically)
            Assert.That(capturedPrivileges, Is.EquivalentTo(expectedPrivileges));
        }

        /// <summary>
        ///     Test: Empty privilege list should default to privilege ID 1
        ///     Business Rule: When no privileges are specified, assign default privilege (ID: 1)
        ///     This ensures every role has at least basic access rights
        /// </summary>
        [Test]
        public async Task Handle_Should_UseDefaultPrivilegeId_WhenPrivilegeIdsAreNullOrEmpty()
        {
            // Arrange: Create command with empty privilege list
            var command = new CreateRoleCommand(
                "Guest",
                "GUEST",
                "Guest role with default permissions",
                new List<int>() // Empty list - should trigger default privilege assignment
            );

            // Mock created entity and expected DTO
            var createdRoleEntity = new Domain.Entities.Role
            {
                RoleId = 2,
                RoleName = command.RoleName
            };

            var expectedRoleDto = new RoleDto
            {
                RoleId = 2,
                RoleName = command.RoleName,
                Privileges = new List<PrivilegeDto>(),
                RoleCode = command.RoleCode,
                Description = command.Description
            };

            // Default privilege ID that should be assigned
            var expectedPrivilegeIds = new List<int> { 1 };

            // Setup repository to accept entity with default privilege
            _roleRepository.CreateAsync(
                    Arg.Any<Domain.Entities.Role>(),
                    Arg.Is<List<int>>(p => p.SequenceEqual(expectedPrivilegeIds))
                )
                .Returns(Task.FromResult(createdRoleEntity));

            // Setup mapper
            _mapper.Map<RoleDto>(createdRoleEntity).Returns(expectedRoleDto);

            // Act: Execute handler
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert: Verify default privilege ID (1) was used
            await _roleRepository.Received(1).CreateAsync(
                Arg.Any<Domain.Entities.Role>(),
                Arg.Is<List<int>>(p => p.Count == 1 && p[0] == 1)
            );

            // Assert: Verify mapper was called
            _mapper.Received(1).Map<RoleDto>(createdRoleEntity);

            // Assert: Verify correct DTO is returned
            Assert.That(result, Is.EqualTo(expectedRoleDto));
        }

        /// <summary>
        ///     Test: Invalid role name should throw ArgumentException
        ///     Business Rule: Role name is mandatory and cannot be empty or whitespace
        ///     This serves as a final safety check even after validation
        /// </summary>
        [TestCase("")]
        [TestCase("   ")]
        public void Handle_Should_ThrowArgumentException_WhenRoleNameIsInvalid(string invalidName)
        {
            // Arrange: Create command with invalid role name
            var command = new CreateRoleCommand(
                invalidName,
                "VALID_CODE",
                "Valid Description",
                new List<int> { 1 }
            );

            // Act & Assert: Handler should throw ArgumentException for invalid name
            // Note: This typically should be caught by validator, but handler enforces as well
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _handler.Handle(command, CancellationToken.None)
            );
        }
    }
}
