using FluentValidation.TestHelper;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.UpdateUser;
namespace IAMService.Application.Test.Features.User.Commands.UpdateUser
{
    /// <summary>
    ///     Unit tests for the UpdateUserCommandValidator class.
    /// </summary>
    [TestFixture]
    public class UpdateUserCommandValidatorTest
    {

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _validator = new UpdateUserCommandValidator();
        }
        /// <summary>
        ///     The validator
        /// </summary>
        private UpdateUserCommandValidator _validator;

        /// <summary>
        ///     Shoulds the have error when user identifier is empty.
        /// </summary>
        [Test]
        public void Should_HaveError_When_UserId_IsEmpty()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.Empty,
                Dto = new UpdateUserRequestDto()
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage("UserId is required.");
        }

        /// <summary>
        ///     Shoulds the have error when email format is invalid.
        /// </summary>
        [Test]
        public void Should_HaveError_When_EmailFormatIsInvalid()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                Dto = new UpdateUserRequestDto { Email = "invalid-email" }
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Dto.Email)
                .WithErrorMessage("Invalid email format.");
        }

        /// <summary>
        ///     Shoulds the have error when phone number is invalid.
        /// </summary>
        [Test]
        public void Should_HaveError_When_PhoneNumberIsInvalid()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                Dto = new UpdateUserRequestDto { PhoneNumber = "12345" }
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Dto.PhoneNumber)
                .WithErrorMessage("Phone number must be 10 digits and start with 0 or +84.");
        }

        /// <summary>
        ///     Shoulds the have error when identity number is invalid.
        /// </summary>
        [Test]
        public void Should_HaveError_When_IdentityNumberIsInvalid()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                Dto = new UpdateUserRequestDto { IdentityNumber = "abc" }
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Dto.IdentityNumber)
                .WithErrorMessage("IdentityNumber must contain 9 to 12 digits.");
        }

        /// <summary>
        ///     Shoulds the have error when date of birth format invalid.
        /// </summary>
        [Test]
        public void Should_HaveError_When_DateOfBirthFormatInvalid()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                Dto = new UpdateUserRequestDto { DateOfBirth = "1990-01-01" }
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Dto.DateOfBirth)
                .WithErrorMessage("DateOfBirth must be in MM/DD/YYYY format.");
        }

        /// <summary>
        ///     Shoulds the have error when no field provided.
        /// </summary>
        [Test]
        public void Should_HaveError_When_NoFieldProvided()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                Dto = new UpdateUserRequestDto()
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Dto)
                .WithErrorMessage("At least one field must be provided for update.");
        }

        /// <summary>
        ///     Shoulds the have error when non admin tries to change privileges.
        /// </summary>
        [Test]
        public void Should_HaveError_When_NonAdmin_TriesToChangePrivileges()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                IsAdmin = false,
                Dto = new UpdateUserRequestDto { PrivilegeIds = new List<int> { 1, 2, 3 } }
            };

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.IsAdmin)
                .WithErrorMessage("Only admin users can modify privileges for other users.");
        }

        /// <summary>
        ///     Shoulds the not have error when admin changes privileges.
        /// </summary>
        [Test]
        public void Should_NotHaveError_When_Admin_ChangesPrivileges()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                IsAdmin = true,
                Dto = new UpdateUserRequestDto { PrivilegeIds = new List<int> { 1, 2 } }
            };

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.IsAdmin);
        }

        /// <summary>
        ///     Shoulds the not have error for valid command.
        /// </summary>
        [Test]
        public void Should_NotHaveError_For_ValidCommand()
        {
            var command = new UpdateUserCommand
            {
                UserId = Guid.NewGuid(),
                IsAdmin = true,
                Dto = new UpdateUserRequestDto
                {
                    FullName = "Test User",
                    Email = "test@example.com",
                    PhoneNumber = "0123456789",
                    Gender = true,
                    IdentityNumber = "123456789012",
                    DateOfBirth = "05/15/1990",
                    Address = "123 Test Street"
                }
            };

            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
