using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
using System.Globalization;
using FluentValidation.Results;

namespace IAMService.Application.Features.User.Commands.CreateUser
{
    /// <summary>
    /// Handler for CreateUserCommand
    /// </summary>
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        /// <summary>
        /// The user repository
        /// </summary>
        private readonly IUserRepository _userRepository;
        /// <summary>
        /// The role repository
        /// </summary>
        private readonly IRoleRepository _roleRepository;
        /// <summary>
        /// The privilege repository
        /// </summary>
        private readonly IPrivilegeRepository _privilegeRepository;
        /// <summary>
        /// The password hasher
        /// </summary>
        private readonly IPasswordHasher _passwordHasher;
        /// <summary>
        /// The email service
        /// </summary>
        private readonly IEmailService _emailService;
        /// <summary>
        /// The audit log service
        /// </summary>
        private readonly IAuditLogService _auditLogService;
        /// <summary>
        /// The mapper
        /// </summary>
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateUserCommandHandler"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="privilegeRepository">The privilege repository.</param>
        /// <param name="passwordHasher">The password hasher.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="auditLogService">The audit log service.</param>
        /// <param name="mapper">The mapper.</param>
        public CreateUserCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IPrivilegeRepository privilegeRepository, IPasswordHasher passwordHasher, IEmailService emailService, IAuditLogService auditLogService, IMapper mapper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _privilegeRepository = privilegeRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _auditLogService = auditLogService;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the CreateUserCommand
        /// </summary>
        /// <param name="request">The create user command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// UserDto with created user information
        /// </returns>
        /// <exception cref="FluentValidation.ValidationException"></exception>
        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var failures = new List<ValidationFailure>();
            // Check if email already exists in the system
            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                failures.Add(new ValidationFailure(
                    nameof(request.Email),
                    $"Email '{request.Email}' is already registered"));
            }
            // Check if identity number already exists in the system
            if (await _userRepository.ExistsByIdentityNumberAsync(request.IdentityNumber))
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
                throw new FluentValidation.ValidationException(failures);
            //Handle password based on user type
            string passwordToUse;
            string? generatedPassword = null;
            if (request.IsPatient)
            {
                generatedPassword = _passwordHasher.GenerateRandomPassword(12);
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
                fullName: request.FullName,
                phoneNumber: request.PhoneNumber,
                email: request.Email,
                hashedPassword: hashedPassword,
                gender: genderBool,
                identityNumber: request.IdentityNumber,
                dateOfBirth: dateOfBirth,
                address: request.Address,
                roleId: roleId,
                isPatient: request.IsPatient
            );
            //Save user to database
            var createdUser = await _userRepository.CreateAsync(newUser);
            //Send notifications and log audit trail
            if (request.IsPatient)
            {
                // AC05: Send email notification to patient with generated password
                await _emailService.SendNewPatientAccountEmailAsync(
                    createdUser.Email,
                    createdUser.FullName,
                    generatedPassword!);

                // AC05: Log audit trail for patient account creation
                await _auditLogService.LogUserCreationAsync(
                    createdUser.UserId,
                    createdUser.Email,
                    "Patient",
                    "System"// TODO: Replace with actual admin/user who created this account
                    );
            }
            else
            {
                // Send welcome email to employee (without password)
                await _emailService.SendNewEmployeeAccountEmailAsync(
                    createdUser.Email,
                    createdUser.FullName);

                // Log audit trail for employee account creation
                await _auditLogService.LogUserCreationAsync(
                    createdUser.UserId,
                    createdUser.Email,
                    "Employee",
                    "System" // TODO: Replace with actual admin/user who created this account
                    );
            }

            //Map entity to DTO and return
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
