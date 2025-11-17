using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using IAMService.Application.DTOs;
using IAMService.Application.IntegrationEvents;
using IAMService.Application.Interfaces;
using IAMService.Application.Interfaces.EventBus;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
namespace IAMService.Application.Features.User.Commands.CreateUser
{
    /// <summary>
    ///     Handler for CreateUserCommand
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        /// <summary>
        ///     The audit log service
        /// </summary>
        private readonly IAuditLogService _auditLogService;
        /// <summary>
        ///     The email service
        /// </summary>
        private readonly IEmailService _emailService;

        private readonly IEventPublisher _eventPublisher;

        private readonly ILogger<CreateUserCommandHandler> _logger;
        /// <summary>
        ///     The mapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        ///     The password hasher
        /// </summary>
        private readonly IPasswordHasher _passwordHasher;
        /// <summary>
        ///     The privilege repository
        /// </summary>
        private readonly IPrivilegeRepository _privilegeRepository;

        private readonly IStringEncryptionService _stringEncryptionService;

        private readonly IUnitOfWork _unitOfWork;
        /// <summary>
        ///     The user repository
        /// </summary>
        private readonly IUserRepository _userRepository;

        //private readonly 

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateUserCommandHandler" /> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="privilegeRepository">The privilege repository.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="auditLogService">The audit log service.</param>
        /// <param name="mapper">The mapper.</param>
        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPrivilegeRepository privilegeRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IAuditLogService auditLogService,
            IMapper mapper,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            IStringEncryptionService stringEncryption,
            ILogger<CreateUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _privilegeRepository = privilegeRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _auditLogService = auditLogService;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
            _stringEncryptionService = stringEncryption;
            _logger = logger;
        }

        /// <summary>
        ///     Handles the CreateUserCommand
        /// </summary>
        /// <param name="request">The create user command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        ///     UserDto with created user information
        /// </returns>
        /// <exception cref="FluentValidation.ValidationException"></exception>
        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var failures = new List<ValidationFailure>();
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            var encryptedEmailForCheck = _stringEncryptionService.EncryptString(normalizedEmail);

            // Check if email already exists in the system (compare encrypted values)
            if (await _userRepository.ExistsByEmailAsync(encryptedEmailForCheck))
            {
                failures.Add(new ValidationFailure(
                    nameof(request.Email),
                    $"Email '{request.Email}' is already registered"));
            }
            // Check if identity number already exists in the system
            var encryptedIdentityForCheck = _stringEncryptionService.EncryptString(request.IdentityNumber);
            if (await _userRepository.ExistsByIdentityNumberAsync(encryptedIdentityForCheck))
            {
                failures.Add(new ValidationFailure(
                    nameof(request.IdentityNumber),
                    $"Identity Number '{request.IdentityNumber}' is already registered"));
            }
            //Validate and prepare privilege IDs
            var privilegeIds = new List<int>();
            if (request.PrivilegeIds == null || !request.PrivilegeIds.Any())
            {
                privilegeIds.Add(1);
            }
            else
            {
                privilegeIds.AddRange(request.PrivilegeIds);
            }
            if (!await _privilegeRepository.AllExistAsync(privilegeIds))
            {
                failures.Add(new ValidationFailure(
                    nameof(request.PrivilegeIds),
                    "One or more privilege IDs are invalid"));
            }
            //Throw validation exception if any failures occurred 
            if (failures.Count > 0)
                throw new ValidationException(failures);
            //Handle password based on user type
            string passwordToUse;
            string? generatedPassword = null;
            if (request.IsPatient)
            {
                generatedPassword = _passwordHasher.GenerateRandomPassword();
                passwordToUse = generatedPassword;
            }
            else
            {
                passwordToUse = request.Password;
            }
            var hashedPassword = _passwordHasher.HashPassword(passwordToUse);
            //Parse date of birth
            var dateOfBirth = DateOnly.ParseExact(request.DateOfBirth, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            //Convert gender string to boolean
            //true = male, false = female
            var genderBool = request.Gender.Trim().ToLower() == "male";
            //Create or find role for the user
            int roleId;
            if (request.IsPatient)
            {
                roleId = 7;
            }
            else
            {
                roleId = 8;
            }
            //Create the user entity

            var newUser = new Domain.Entities.User(
                request.FullName,
                request.PhoneNumber,
                request.Email,
                hashedPassword,
                genderBool,
                request.IdentityNumber,
                dateOfBirth,
                request.Address,
                roleId,
                request.IsPatient
            );

            var newUserHold = newUser.Adapt<Domain.Entities.User>();

            newUser.FullName = _stringEncryptionService.EncryptString(newUser.FullName);
            newUser.PhoneNumber = _stringEncryptionService.EncryptString(newUser.PhoneNumber);
            newUser.Email = _stringEncryptionService.EncryptString(newUser.Email);
            newUser.IdentityNumber = _stringEncryptionService.EncryptString(newUser.IdentityNumber);
            newUser.Address = _stringEncryptionService.EncryptString(newUser.Address);

            // Save user to database
            // Manage transaction
            var createdUser = await _userRepository.CreateAsync(newUser);
            _logger.LogInformation("Create user successfully: {UserId}", createdUser.UserId);
            if (newUserHold.IsPatient)
            {
                // Publish event to 
                var userCreatedIntegration = newUserHold.Adapt<UserCreatedIntegrationEvent>();
                await _eventPublisher.PublishAsync(userCreatedIntegration);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //Send notifications and log audit trail
            // Decrypt values for outbound operations (emails, audit logs, DTOs)
            var decryptedEmail = _stringEncryptionService.DecryptString(createdUser.Email);
            var decryptedFullName = _stringEncryptionService.DecryptString(createdUser.FullName);
            var decryptedPhone = _stringEncryptionService.DecryptString(createdUser.PhoneNumber);
            var decryptedIdentity = _stringEncryptionService.DecryptString(createdUser.IdentityNumber);
            var decryptedAddress = _stringEncryptionService.DecryptString(createdUser.Address);
            if (request.IsPatient)
            {
                // AC05: Send email notification to patient with generated password
                await _emailService.SendNewPatientAccountEmailAsync(
                    decryptedEmail,
                    decryptedFullName,
                    generatedPassword!);

                // AC05: Log audit trail for patient account creation
                await _auditLogService.LogUserCreationAsync(
                    createdUser.UserId,
                    createdUser.Email,
                    "Patient",
                    "System" // TODO: Replace with actual admin/user who created this account
                );
            }
            else
            {
                // Send welcome email to employee (without password)
                await _emailService.SendNewEmployeeAccountEmailAsync(
                    decryptedEmail,
                    decryptedFullName);

                // Log audit trail for employee account creation
                await _auditLogService.LogUserCreationAsync(
                    createdUser.UserId,
                    decryptedEmail,
                    "Employee",
                    "System" // TODO: Replace with actual admin/user who created this account
                );
            }

            //Map entity to DTO and return
            // Replace encrypted values on the entity before mapping so DTOs contain plaintext
            createdUser.Email = decryptedEmail;
            createdUser.FullName = decryptedFullName;
            createdUser.PhoneNumber = decryptedPhone;
            createdUser.IdentityNumber = decryptedIdentity;
            createdUser.Address = decryptedAddress;
            var userDto = _mapper.Map<UserDto>(createdUser);
            // Include generated password in response only for patient accounts
            if (request.IsPatient && generatedPassword != null)
            {
                userDto = userDto with { GeneratedPassword = generatedPassword };
            }
            return userDto;

        }
    }
}
