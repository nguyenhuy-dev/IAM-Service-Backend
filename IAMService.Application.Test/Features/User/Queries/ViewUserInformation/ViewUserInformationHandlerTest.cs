using AutoMapper;
using FluentAssertions;
using IAMService.Application.DTOs;
using IAMService.Application.Exceptions;
using IAMService.Application.Features.User.Queries.ViewUserInformation;
using IAMService.Application.Interfaces;
using IAMService.Application.Mappings;
using NSubstitute;
namespace IAMService.Application.Test.Features.User.Queries.ViewUserInformation
{
    /// <summary>
    /// Unit tests for <see cref="ViewUserInformationHandler"/>
    /// </summary>
    [TestFixture]
    public class ViewUserInformationHandlerTests
    {
        /// <summary>
        /// The user repository
        /// </summary>
        private IUserRepository _userRepository;
        /// <summary>
        /// The mapper
        /// </summary>
        private IMapper _mapper;
        /// <summary>
        /// The handler
        /// </summary>
        private ViewUserInformationHandler _handler;

        /// <summary>
        /// Fixtures the setup.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _handler = new ViewUserInformationHandler(_userRepository, _mapper);
        }

        /// <summary>
        /// Handles the user exists admin can view other user returns user response dto.
        /// </summary>
        [Test]
        public async Task Handle_UserExists_AdminCanViewOtherUser_ReturnsUserResponseDto()
        {
            // Arrange
            var targetUser = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                FullName = "Target User",
                Email = "target@example.com",
                PhoneNumber = "0123456789",
                Address = "Hanoi",
                Gender = true,
                IdentityNumber = "123456789012",
                DateOfBirth = new DateOnly(2000, 1, 1),
                Role = new Domain.Entities.Role { RoleId = 2, RoleName = "User" }
            };

            var currentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                RoleName = "Admin"
            };

            _userRepository.GetByIdAsync(targetUser.UserId).Returns(targetUser);

            var query = new ViewUserInformationQuery
            {
                TargetUserId = targetUser.UserId,
                CurrentUser = currentUser
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Target User");
            result.Email.Should().Be("target@example.com");
            await _userRepository.Received(1).GetByIdAsync(targetUser.UserId);
        }

        /// <summary>
        /// Handles the user exists current user is self returns user response dto.
        /// </summary>
        [Test]
        public async Task Handle_UserExists_CurrentUserIsSelf_ReturnsUserResponseDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var targetUser = new Domain.Entities.User
            {
                UserId = userId,
                FullName = "Self User",
                Email = "self@example.com",
                PhoneNumber = "0123456789",
                Address = "HCM",
                Gender = false,
                IdentityNumber = "987654321098",
                DateOfBirth = new DateOnly(1999, 5, 5),
                Role = new Domain.Entities.Role { RoleId = 3, RoleName = "User" }
            };

            var currentUser = new CurrentUserDto
            {
                UserId = userId,
                RoleName = "User"
            };

            _userRepository.GetByIdAsync(userId).Returns(targetUser);

            var query = new ViewUserInformationQuery
            {
                TargetUserId = userId,
                CurrentUser = currentUser
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Self User");
            result.Email.Should().Be("self@example.com");
            await _userRepository.Received(1).GetByIdAsync(userId);
        }

        /// <summary>
        /// Handles the user does not exist throws not found exception.
        /// </summary>
        [Test]
        public void Handle_UserDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var currentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                RoleName = "Admin"
            };

            _userRepository.GetByIdAsync(targetUserId).Returns((Domain.Entities.User)null!);

            var query = new ViewUserInformationQuery
            {
                TargetUserId = targetUserId,
                CurrentUser = currentUser
            };

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("User not found or has been deleted.");
            _userRepository.Received(1).GetByIdAsync(targetUserId);
        }

        /// <summary>
        /// Handles the non admin tries to view other user throws forbidden access exception.
        /// </summary>
        [Test]
        public void Handle_NonAdminTriesToViewOtherUser_ThrowsForbiddenAccessException()
        {
            // Arrange
            var targetUser = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                FullName = "Target User",
                Email = "target@example.com",
                Role = new Domain.Entities.Role { RoleId = 2, RoleName = "User" }
            };

            var currentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                RoleName = "User"
            };

            _userRepository.GetByIdAsync(targetUser.UserId).Returns(targetUser);

            var query = new ViewUserInformationQuery
            {
                TargetUserId = targetUser.UserId,
                CurrentUser = currentUser
            };

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<ForbiddenAccessException>()
                .WithMessage("You do not have permission to view other users' information.");
            _userRepository.Received(1).GetByIdAsync(targetUser.UserId);
        }

        /// <summary>
        /// Handles the manager can view other user returns user response dto.
        /// </summary>
        [Test]
        public async Task Handle_ManagerCanViewOtherUser_ReturnsUserResponseDto()
        {
            // Arrange
            var targetUser = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                FullName = "Normal User",
                Email = "normal@example.com",
                Role = new Domain.Entities.Role { RoleId = 2, RoleName = "User" }
            };

            var currentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                RoleName = "Manager"
            };

            _userRepository.GetByIdAsync(targetUser.UserId).Returns(targetUser);

            var query = new ViewUserInformationQuery
            {
                TargetUserId = targetUser.UserId,
                CurrentUser = currentUser
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Normal User");
            result.Email.Should().Be("normal@example.com");
            await _userRepository.Received(1).GetByIdAsync(targetUser.UserId);
        }
    }
}
