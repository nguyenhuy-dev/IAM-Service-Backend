using AutoMapper;
using FluentAssertions;
using FluentValidation;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.CreateUser;
using IAMService.Application.Interfaces;
using NSubstitute;
namespace IAMService.Application.Test.Features.User.Commands.CreateUser
{
    /// <summary>
    /// Unit tests for the CreateUserCommandHandler class.
    /// Tests cover both employee and patient user creation scenarios.
    /// 
    /// Test cases include:
    /// 1. Creating an employee user with valid data and password
    /// 2. Creating a patient user with auto-generated password
    /// 3. Validation for existing email
    /// 4. Validation for existing identity number
    /// </summary>
    [TestFixture]
    public class CreateUserCommandHandlerTests
    {
        /// <summary>
        /// The user repository
        /// </summary>
        private IUserRepository _userRepository;
        /// <summary>
        /// The role repository
        /// </summary>
        private IRoleRepository _roleRepository;
        /// <summary>
        /// The privilege repository
        /// </summary>
        private IPrivilegeRepository _privilegeRepository;
        /// <summary>
        /// The password hasher
        /// </summary>
        private IPasswordHasher _passwordHasher;
        /// <summary>
        /// The email service
        /// </summary>
        private IEmailService _emailService;
        /// <summary>
        /// The audit log service
        /// </summary>
        private IAuditLogService _auditLogService;
        /// <summary>
        /// The mapper
        /// </summary>
        private IMapper _mapper;
        /// <summary>
        /// The handler
        /// </summary>
        private CreateUserCommandHandler _handler;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _roleRepository = Substitute.For<IRoleRepository>();
            _privilegeRepository = Substitute.For<IPrivilegeRepository>();
            _passwordHasher = Substitute.For<IPasswordHasher>();
            _emailService = Substitute.For<IEmailService>();
            _auditLogService = Substitute.For<IAuditLogService>();
            _mapper = Substitute.For<IMapper>();
            _handler = new CreateUserCommandHandler(
                _userRepository,
                _roleRepository,
                _privilegeRepository,
                _passwordHasher,
                _emailService,
                _auditLogService,
                _mapper
            );
        }

        /// <summary>
        /// Handles the valid employee user creates user successfully.
        /// </summary>
        [Test]
        public async Task Handle_ValidEmployeeUser_CreatesUserSuccessfully()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test User",
                IdentityNumber = "123456789012",
                Gender = "Male",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false,
                PrivilegeIds = new[] { 1, 2 }
            };

            _userRepository.ExistsByEmailAsync(command.Email).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(command.IdentityNumber).Returns(false);

            var hashedPassword = "hashedPassword123";
            _passwordHasher.HashPassword(command.Password).Returns(hashedPassword);

            var defaultRole = new Domain.Entities.Role
            {
                RoleId = 1,
                RoleName = "Default Role",
                RoleCode = "DEFAULT",
                Description = "Default role description"
            };
            _roleRepository.GetByCodeAsync("READ_ONLY").Returns(defaultRole);
            _privilegeRepository.AllExistAsync(Arg.Any<IEnumerable<int>>()).Returns(true);

            var createdUser = new Domain.Entities.User
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                FullName = command.FullName
            };

            _userRepository.CreateAsync(Arg.Any<Domain.Entities.User>()).Returns(createdUser);

            var expectedDto = new UserDto
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                FullName = createdUser.FullName,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                IdentityNumber = command.IdentityNumber,
                Address = command.Address,
                Role = new RoleDto
                {
                    RoleId = 1,
                    RoleName = "Default Role",
                    RoleCode = "DEFAULT",
                    Description = "Default role description",
                    Privileges = new List<PrivilegeDto>()
                },
                DateOfBirth = DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                Age = 33,
                IsPatient = false,
                NeedsVerification = true
            };

            _mapper.Map<UserDto>(createdUser).Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDto);

            await _userRepository.Received(1).CreateAsync(Arg.Any<Domain.Entities.User>());
            await _emailService.Received(1).SendNewEmployeeAccountEmailAsync(
                command.Email,
                command.FullName,
                Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Handles the valid patient user creates user with automatic generated password.
        /// </summary>
        [Test]
        public async Task Handle_ValidPatientUser_CreatesUserWithAutoGeneratedPassword()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "patient@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test Patient",
                IdentityNumber = "123456789012",
                Gender = "Female",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = null,
                IsPatient = true,
                PrivilegeIds = null
            };

            _userRepository.ExistsByEmailAsync(command.Email).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(command.IdentityNumber).Returns(false);
            _privilegeRepository.AllExistAsync(Arg.Any<IEnumerable<int>>()).Returns(true);

            // Mock password generation and hashing for patient
            var generatedPassword = "AutoGenerated123!";
            var hashedPassword = "hashedAutoGenerated123";
            _passwordHasher.GenerateRandomPassword(12).Returns(generatedPassword);
            _passwordHasher.HashPassword(generatedPassword).Returns(hashedPassword);

            var createdUser = new Domain.Entities.User(
                fullName: command.FullName,
                phoneNumber: command.PhoneNumber,
                email: command.Email,
                hashedPassword: hashedPassword,
                gender: command.Gender.ToLower() == "male",
                identityNumber: command.IdentityNumber,
                dateOfBirth: DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                address: command.Address,
                roleId: 7, // Patient role ID
                isPatient: true
            );

            _userRepository.CreateAsync(Arg.Any<Domain.Entities.User>()).Returns(createdUser);

            var expectedDto = new UserDto
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                FullName = createdUser.FullName,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                IdentityNumber = command.IdentityNumber,
                Address = command.Address,
                Role = new RoleDto
                {
                    RoleId = 7,
                    RoleName = "Patient",
                    RoleCode = "PATIENT",
                    Description = "Patient role for laboratory system",
                    Privileges = new List<PrivilegeDto>()
                },
                DateOfBirth = DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                Age = 33,
                IsPatient = true,
                NeedsVerification = true,
                GeneratedPassword = generatedPassword
            };

            _mapper.Map<UserDto>(createdUser).Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedDto);

            await _userRepository.Received(1).CreateAsync(Arg.Is<Domain.Entities.User>(u =>
                u.Email == command.Email &&
                u.FullName == command.FullName &&
                u.RoleId == 7));

            await _emailService.Received(1).SendNewPatientAccountEmailAsync(
                command.Email,
                command.FullName,
                generatedPassword);
        }


        /// <summary>
        /// Handles the existing email throws validation exception.
        /// </summary>
        [Test]
        public async Task Handle_ExistingEmail_ThrowsValidationException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "existing@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test User",
                IdentityNumber = "123456789012",
                Gender = "Male",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false
            };

            _userRepository.ExistsByEmailAsync(command.Email).Returns(true);

            // Act
            Func<Task<UserDto>> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Email*already registered*");
        }

        /// <summary>
        /// Handles the existing identity number throws validation exception.
        /// </summary>
        [Test]
        public async Task Handle_ExistingIdentityNumber_ThrowsValidationException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test User",
                IdentityNumber = "123456789012",
                Gender = "Male",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false
            };

            _userRepository.ExistsByEmailAsync(command.Email).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(command.IdentityNumber).Returns(true);

            // Act
            Func<Task<UserDto>> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Identity Number*already registered*");
        }
    }
}