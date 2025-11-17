using AutoMapper;
using FluentAssertions;
using FluentValidation;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.CreateUser;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.EventBus;
using IAMService.Application.Interfaces.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;
namespace IAMService.Application.Test.Features.User.Commands.CreateUser
{
    /// <summary>
    ///     Unit tests for the CreateUserCommandHandler class.
    ///     Tests cover both employee and patient user creation scenarios.
    ///     Test cases include:
    ///     1. Creating an employee user with valid data and password
    ///     2. Creating a patient user with auto-generated password
    ///     3. Validation for existing email
    ///     4. Validation for existing identity number
    ///     5. Encryption/Decryption of sensitive data
    ///     6. Integration event publishing for patients
    ///     7. Transaction handling with UnitOfWork
    /// </summary>
    [TestFixture]
    public class CreateUserCommandHandlerTests
    {

        /// <summary>
        ///     Setups this instance.
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
            _eventPublisher = Substitute.For<IEventPublisher>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _stringEncryptionService = Substitute.For<IStringEncryptionService>();
            _logger = Substitute.For<ILogger<CreateUserCommandHandler>>();

            _handler = new CreateUserCommandHandler(
                _userRepository,
                _roleRepository,
                _privilegeRepository,
                _passwordHasher,
                _emailService,
                _auditLogService,
                _mapper,
                _eventPublisher,
                _unitOfWork,
                _stringEncryptionService,
                _logger
            );
        }
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IPrivilegeRepository _privilegeRepository;
        private IPasswordHasher _passwordHasher;
        private IEmailService _emailService;
        private IAuditLogService _auditLogService;
        private IMapper _mapper;
        private IEventPublisher _eventPublisher;
        private IUnitOfWork _unitOfWork;
        private IStringEncryptionService _stringEncryptionService;
        private ILogger<CreateUserCommandHandler> _logger;
        private CreateUserCommandHandler _handler;

        /// <summary>
        ///     Handles the valid employee user creates user successfully with encryption.
        /// </summary>
        [Test]
        public async Task Handle_ValidEmployeeUser_CreatesUserSuccessfullyWithEncryption()
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

            // Mock encryption service
            var encryptedEmail = "encrypted@test.com"; // Must be valid email format
            var encryptedIdentity = "123456789012"; // Must be valid 12-digit identity
            var encryptedFullName = "encrypted_fullname";
            var encryptedPhone = "0123456789"; // Must be valid 10-digit phone
            var encryptedAddress = "encrypted_address";

            _stringEncryptionService.EncryptString("test@example.com").Returns(encryptedEmail);
            _stringEncryptionService.EncryptString("123456789012").Returns(encryptedIdentity);
            _stringEncryptionService.EncryptString("Test User").Returns(encryptedFullName);
            _stringEncryptionService.EncryptString("0123456789").Returns(encryptedPhone);
            _stringEncryptionService.EncryptString("123 Test St").Returns(encryptedAddress);

            _stringEncryptionService.DecryptString(encryptedEmail).Returns("test@example.com");
            _stringEncryptionService.DecryptString(encryptedFullName).Returns("Test User");
            _stringEncryptionService.DecryptString(encryptedPhone).Returns("0123456789");
            _stringEncryptionService.DecryptString(encryptedIdentity).Returns("123456789012");
            _stringEncryptionService.DecryptString(encryptedAddress).Returns("123 Test St");

            _userRepository.ExistsByEmailAsync(encryptedEmail).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(encryptedIdentity).Returns(false);

            var hashedPassword = "hashedPassword123";
            _passwordHasher.HashPassword(command.Password).Returns(hashedPassword);

            _privilegeRepository.AllExistAsync(Arg.Any<IEnumerable<int>>()).Returns(true);

            var createdUser = new Domain.Entities.User(
                encryptedFullName,
                encryptedPhone,
                encryptedEmail,
                hashedPassword,
                true, // Male
                encryptedIdentity,
                DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                encryptedAddress,
                8
            );

            _userRepository.CreateAsync(Arg.Any<Domain.Entities.User>()).Returns(createdUser);
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var expectedDto = new UserDto
            {
                UserId = createdUser.UserId,
                Email = command.Email,
                FullName = command.FullName,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                IdentityNumber = command.IdentityNumber,
                Address = command.Address,
                Role = new RoleDto
                {
                    RoleId = 8,
                    RoleName = "Employee",
                    RoleCode = "EMPLOYEE",
                    Description = "Employee role",
                    Privileges = new List<PrivilegeDto>()
                },
                DateOfBirth = DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                Age = DateTime.Now.Year - 1990,
                IsPatient = false,
                NeedsVerification = false
            };

            _mapper.Map<UserDto>(Arg.Any<Domain.Entities.User>()).Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(command.Email);
            result.FullName.Should().Be(command.FullName);

            await _userRepository.Received(1).CreateAsync(Arg.Any<Domain.Entities.User>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _emailService.Received(1).SendNewEmployeeAccountEmailAsync(
                command.Email,
                command.FullName);

            // Verify encryption was called (called multiple times: once for existence check, once for user creation)
            _stringEncryptionService.Received().EncryptString(command.Email.Trim().ToLowerInvariant());
            _stringEncryptionService.Received().EncryptString(command.IdentityNumber);
        }

        /// <summary>
        ///     Handles the valid patient user creates user with auto-generated password and publishes integration event.
        /// </summary>
        [Test]
        public async Task Handle_ValidPatientUser_CreatesUserWithAutoGeneratedPasswordAndPublishesEvent()
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
                PrivilegeIds = null // Will default to privilege 1
            };

            // Mock encryption
            _stringEncryptionService.EncryptString(Arg.Any<string>()).Returns(x => $"enc_{x[0]}");
            _stringEncryptionService.DecryptString(Arg.Any<string>()).Returns(x => x[0].ToString().Replace("enc_", ""));

            _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(Arg.Any<string>()).Returns(false);
            _privilegeRepository.AllExistAsync(Arg.Is<IEnumerable<int>>(p => p.Contains(1))).Returns(true);

            // Mock password generation and hashing for patient
            var generatedPassword = "AutoGenerated123!";
            var hashedPassword = "hashedAutoGenerated123";
            _passwordHasher.GenerateRandomPassword().Returns(generatedPassword);
            _passwordHasher.HashPassword(generatedPassword).Returns(hashedPassword);

            var createdUser = new Domain.Entities.User(
                "enc_fullname",
                "0987654321", // Valid 10-digit phone number
                "patient@enc.com", // Valid email format
                hashedPassword,
                false, // Female
                "987654321098", // Valid 12-digit identity
                DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                "enc_address",
                7, // Patient role ID
                true
            );

            // Mock DecryptString for createdUser fields
            _stringEncryptionService.DecryptString("patient@enc.com").Returns(command.Email);
            _stringEncryptionService.DecryptString("enc_fullname").Returns(command.FullName);
            _stringEncryptionService.DecryptString("0987654321").Returns(command.PhoneNumber);
            _stringEncryptionService.DecryptString("987654321098").Returns(command.IdentityNumber);
            _stringEncryptionService.DecryptString("enc_address").Returns(command.Address);

            _userRepository.CreateAsync(Arg.Any<Domain.Entities.User>()).Returns(createdUser);
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

            var expectedDto = new UserDto
            {
                UserId = createdUser.UserId,
                Email = command.Email,
                FullName = command.FullName,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                IdentityNumber = command.IdentityNumber,
                Address = command.Address,
                Role = new RoleDto
                {
                    RoleId = 7,
                    RoleName = "Patient",
                    RoleCode = "PATIENT",
                    Description = "Patient role",
                    Privileges = new List<PrivilegeDto>()
                },
                DateOfBirth = DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                Age = DateTime.Now.Year - 1990,
                IsPatient = true,
                NeedsVerification = false,
                GeneratedPassword = generatedPassword
            };

            _mapper.Map<UserDto>(Arg.Any<Domain.Entities.User>()).Returns(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsPatient.Should().BeTrue();
            result.GeneratedPassword.Should().Be(generatedPassword);

            await _userRepository.Received(1).CreateAsync(Arg.Is<Domain.Entities.User>(u => u.RoleId == 7));
            await _eventPublisher.Received(1).PublishAsync(Arg.Any<IntegrationEvent>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _emailService.Received(1).SendNewPatientAccountEmailAsync(
                command.Email,
                command.FullName,
                generatedPassword);

            _passwordHasher.Received(1).GenerateRandomPassword();
        }

        /// <summary>
        ///     Handles the existing email throws validation exception with encrypted check.
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

            var encryptedEmail = "encrypted_existing_email";
            _stringEncryptionService.EncryptString("existing@example.com").Returns(encryptedEmail);
            _userRepository.ExistsByEmailAsync(encryptedEmail).Returns(true);

            // Act
            Func<Task<UserDto>> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Email*already registered*");
        }

        /// <summary>
        ///     Handles the existing identity number throws validation exception with encrypted check.
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

            var encryptedEmail = "enc_test_email";
            var encryptedIdentity = "enc_existing_identity";

            _stringEncryptionService.EncryptString("test@example.com").Returns(encryptedEmail);
            _stringEncryptionService.EncryptString("123456789012").Returns(encryptedIdentity);

            _userRepository.ExistsByEmailAsync(encryptedEmail).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(encryptedIdentity).Returns(true);

            // Act
            Func<Task<UserDto>> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Identity Number*already registered*");
        }

        /// <summary>
        ///     Handles invalid privilege IDs throws validation exception.
        /// </summary>
        [Test]
        public async Task Handle_InvalidPrivilegeIds_ThrowsValidationException()
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
                PrivilegeIds = new[] { 999, 888 }
            };

            _stringEncryptionService.EncryptString(Arg.Any<string>()).Returns("encrypted");
            _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(Arg.Any<string>()).Returns(false);
            _privilegeRepository.AllExistAsync(Arg.Any<IEnumerable<int>>()).Returns(false);

            // Act
            Func<Task<UserDto>> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*privilege*invalid*");
        }

        /// <summary>
        ///     Handles null privilege IDs defaults to privilege 1.
        /// </summary>
        [Test]
        public async Task Handle_NullPrivilegeIds_DefaultsToPrivilege1()
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
                PrivilegeIds = null
            };

            _stringEncryptionService.EncryptString(Arg.Any<string>()).Returns(x => $"enc_{x[0]}");
            _stringEncryptionService.DecryptString(Arg.Any<string>()).Returns(x => x[0].ToString().Replace("enc_", ""));

            _userRepository.ExistsByEmailAsync(Arg.Any<string>()).Returns(false);
            _userRepository.ExistsByIdentityNumberAsync(Arg.Any<string>()).Returns(false);

            var receivedPrivilegeCheck = false;
            _privilegeRepository.AllExistAsync(Arg.Is<IEnumerable<int>>(p => p.Contains(1)))
                .Returns(x =>
                {
                    receivedPrivilegeCheck = true;
                    return true;
                });

            _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
            _userRepository.CreateAsync(Arg.Any<Domain.Entities.User>()).Returns(new Domain.Entities.User(
                "enc", "0999999999", "test@enc.com", "hashed", true, "111111111111", DateOnly.FromDateTime(DateTime.Now), "enc", 8));
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));
            _mapper.Map<UserDto>(Arg.Any<Domain.Entities.User>()).Returns(new UserDto
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                FullName = "Test User",
                PhoneNumber = "0123456789",
                Gender = "Male",
                IdentityNumber = "123456789012",
                Address = "123 Test St",
                Role = new RoleDto
                {
                    RoleId = 8,
                    RoleName = "Employee",
                    RoleCode = "EMPLOYEE",
                    Description = "Employee role",
                    Privileges = new List<PrivilegeDto>()
                }
            });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            receivedPrivilegeCheck.Should().BeTrue("Handler should check if privilege 1 exists when PrivilegeIds is null");
            await _privilegeRepository.Received(1).AllExistAsync(Arg.Is<IEnumerable<int>>(p => p.Contains(1)));
        }
    }
}
