using FluentValidation.TestHelper;
using IAMService.Application.Features.Auth.Commands.Login;
namespace IAMService.Application.Test.Features.Auth.Commands.Login
{
    [TestFixture]
    public class LoginCommandValidatorTests
    {

        [SetUp]
        public void SetUp()
        {
            _validator = new LoginCommandValidator();
        }
        private LoginCommandValidator _validator = null!;

        [Test]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            // Arrange
            var command = new LoginCommand("", "Password123");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Test]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var command = new LoginCommand("invalidemail", "Password123");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Test]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            var command = new LoginCommand("test@example.com", "");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Test]
        public void Should_Have_Error_When_Password_Is_Shorter_Than_8_Characters()
        {
            var command = new LoginCommand("test@example.com", "Aa12");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Test]
        public void Should_Have_Error_When_Password_Missing_Uppercase()
        {
            var command = new LoginCommand("test@example.com", "password123");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must include uppercase & lowercase letters and numbers.");
        }

        [Test]
        public void Should_Have_Error_When_Password_Missing_Lowercase()
        {
            var command = new LoginCommand("test@example.com", "PASSWORD123");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must include uppercase & lowercase letters and numbers.");
        }

        [Test]
        public void Should_Have_Error_When_Password_Missing_Number()
        {
            var command = new LoginCommand("test@example.com", "Password");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage("Password must include uppercase & lowercase letters and numbers.");
        }

        [Test]
        public void Should_Pass_When_Valid_Email_And_Password()
        {
            var command = new LoginCommand("valid@example.com", "Password123");

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
