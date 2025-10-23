using AutoMapper;
using FluentValidation;
using IAMService.Application.DTOs;
using IAMService.Application.Exceptions;
using IAMService.Application.Features.Role.Commands.UpdateRole;
using IAMService.Application.Interfaces;
using NSubstitute;
namespace IAMService.Application.Test.Features.Role.Commands.UpdateRole
{
    /// <summary>
    /// The update role command handler test class
    /// </summary>
    [TestFixture]
    public class UpdateRoleCommandHandlerTest
    {
        /// <summary>
        /// The role repository
        /// </summary>
        private IRoleRepository _roleRepository;
        /// <summary>
        /// The mapper
        /// </summary>
        private IMapper _mapper;
        /// <summary>
        /// The handler
        /// </summary>
        private UpdateRoleCommandHandler _handler;

        /// <summary>
        /// Setup this instance
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _mapper = Substitute.For<IMapper>();
            _handler = new UpdateRoleCommandHandler(_roleRepository, _mapper);
        }

        /// <summary>
        /// Tests that handle should update role when command is valid
        /// </summary>
        [Test]
        public async Task Handle_Should_UpdateRole_WhenCommandIsValid()
        {
            // ## Arrange ##
            var command = new UpdateRoleCommand(1, "New Name", "NEW_CODE", "New Desc", new List<int> { 10 });
            var existingRole = new Domain.Entities.Role(1, "Old Name", "OLD_CODE", "Old Desc"); // IsDefault is false by default
            var expectedDto = new RoleDto { RoleId = 1, RoleName = "New Name", Privileges = [], RoleCode = "NEW_CODE", Description = "New Desc"};

            _roleRepository.GetByIdAsync(command.RoleId).Returns(existingRole);
            _roleRepository.UpdateAsync(Arg.Any<Domain.Entities.Role>(), Arg.Any<List<int>>()).Returns(existingRole);
            _mapper.Map<RoleDto>(existingRole).Returns(expectedDto);

            // ## Act ##
            var result = await _handler.Handle(command, CancellationToken.None);

            // ## Assert ##
            Assert.That(result, Is.EqualTo(expectedDto));
            await _roleRepository.Received(1).UpdateAsync(
                Arg.Is<Domain.Entities.Role>(r => r.RoleName == command.RoleName && r.RoleCode == command.RoleCode),
                Arg.Is<List<int>>(p => p.SequenceEqual(command.PrivilegeIds))
            );
        }

        /// <summary>
        /// Tests that handle should throw not found exception when role does not exist
        /// </summary>
        [Test]
        public void Handle_Should_ThrowNotFoundException_WhenRoleDoesNotExist()
        {
            // ## Arrange ##
            var command = new UpdateRoleCommand(999, "Name", "CODE", "Desc", null);
            _roleRepository.GetByIdAsync(command.RoleId).Returns((Domain.Entities.Role)null);

            // ## Act & Assert ##
            Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        /// <summary>
        /// Tests that handle should throw validation exception when role is default
        /// </summary>
        [Test]
        public void Handle_Should_ThrowValidationException_WhenRoleIsDefault()
        {
            // ## Arrange ##
            var command = new UpdateRoleCommand(1, "Name", "CODE", "Desc", null);
            var defaultRole = new Domain.Entities.Role { RoleId = 1, IsDefault = true };
            _roleRepository.GetByIdAsync(command.RoleId).Returns(defaultRole);

            // ## Act & Assert ##
            var ex = Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.That(ex.Message, Is.EqualTo("Default roles cannot be deleted."));
        }
    }
}