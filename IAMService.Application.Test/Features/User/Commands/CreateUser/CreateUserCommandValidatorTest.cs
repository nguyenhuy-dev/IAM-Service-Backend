using FluentAssertions;
using IAMService.Application.Features.User.Commands.CreateUser;
namespace IAMService.Application.Test.Features.User.Commands.CreateUser
{
    /// <summary>
    ///     Unit tests for the CreateUserCommandValidator class.
    ///     Tests the validation rules for user creation requests.
    ///     Test cases include:
    ///     1. Valid employee user data validation
    ///     2. Valid patient user data validation
    ///     3. Email format validation
    ///     4. Password requirements validation
    ///     5. Required field validations
    ///     6. Date format validations
    ///     7. Phone number format validation
    ///     8. Identity number format validation
    /// </summary>
    [TestFixture]
    public class CreateUserCommandValidatorTests
    {

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _validator = new CreateUserCommandValidator();
        }
        /// <summary>
        ///     The validator
        /// </summary>
        private CreateUserCommandValidator _validator;

        /// <summary>
        ///     Validates the valid employee user should not have validation error.
        /// </summary>
        [Test]
        public async Task Validate_ValidEmployeeUser_ShouldNotHaveValidationError()
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

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        /// <summary>
        ///     Validates the valid patient user should not have validation error.
        /// </summary>
        [Test]
        public async Task Validate_ValidPatientUser_ShouldNotHaveValidationError()
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
                IsPatient = true
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        /// <summary>
        ///     Validates the invalid email should have validation error.
        /// </summary>
        /// <param name="invalidEmail">The invalid email.</param>
        [TestCase(""), TestCase("invalid"), TestCase("invalid@"), TestCase("@invalid.com")]
        public async Task Validate_InvalidEmail_ShouldHaveValidationError(string invalidEmail)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = invalidEmail,
                PhoneNumber = "0123456789",
                FullName = "Test User",
                IdentityNumber = "123456789012",
                Gender = "Male",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Email));
        }

        /// <summary>
        ///     Validates the invalid phone number should have validation error.
        /// </summary>
        /// <param name="invalidPhone">The invalid phone.</param>
        [TestCase(""), TestCase("123456789"), TestCase("12345678901"), TestCase("1234567890")]
        // 9 digits
        // 11 digits
         // Doesn't start with 0
        public async Task Validate_InvalidPhoneNumber_ShouldHaveValidationError(string invalidPhone)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                PhoneNumber = invalidPhone,
                FullName = "Test User",
                IdentityNumber = "123456789012",
                Gender = "Male",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.PhoneNumber));
        }

        /// <summary>
        ///     Validates the invalid identity number should have validation error.
        /// </summary>
        /// <param name="invalidId">The invalid identifier.</param>
        [TestCase(""), TestCase("12345678901"), TestCase("1234567890123"), TestCase("12345abcd890")]
        // 11 digits
        // 13 digits
         // Contains letters
        public async Task Validate_InvalidIdentityNumber_ShouldHaveValidationError(string invalidId)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test User",
                IdentityNumber = invalidId,
                Gender = "Male",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.IdentityNumber));
        }

        /// <summary>
        ///     Validates the invalid gender should have validation error.
        /// </summary>
        /// <param name="invalidGender">The invalid gender.</param>
        [TestCase(""), TestCase("Other"), TestCase("M"), TestCase("F")]
        public async Task Validate_InvalidGender_ShouldHaveValidationError(string invalidGender)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test User",
                IdentityNumber = "123456789012",
                Gender = invalidGender,
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Gender));
        }

        /// <summary>
        ///     Validates the invalid date of birth should have validation error.
        /// </summary>
        /// <param name="invalidDate">The invalid date.</param>
        [TestCase(""), TestCase("13/13/2020"), TestCase("2020/01/01"), TestCase("01-01-2020"), TestCase("invalid")]
        public async Task Validate_InvalidDateOfBirth_ShouldHaveValidationError(string invalidDate)
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
                DateOfBirth = invalidDate,
                Password = "StrongP@ss123",
                IsPatient = false
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.DateOfBirth));
        }

        /// <summary>
        ///     Validates the patient with password should have validation error.
        /// </summary>
        [Test]
        public async Task Validate_PatientWithPassword_ShouldHaveValidationError()
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
                IsPatient = true // Patient shouldn't have password
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Password));
        }

        /// <summary>
        ///     Validates the employee without password should have validation error.
        /// </summary>
        [Test]
        public async Task Validate_EmployeeWithoutPassword_ShouldHaveValidationError()
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
                Password = null,
                IsPatient = false // Employee should have password
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Password));
        }

        /// <summary>
        ///     Validates the weak password should have validation error.
        /// </summary>
        /// <param name="weakPassword">The weak password.</param>
        [TestCase("short"), TestCase("nouppercase123!"), TestCase("NOLOWERCASE123!"), TestCase("NoSpecialChar123"), TestCase("No@Numbers")]
        public async Task Validate_WeakPassword_ShouldHaveValidationError(string weakPassword)
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
                Password = weakPassword,
                IsPatient = false
            };

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.PropertyName == nameof(command.Password));
        }
    }
}
