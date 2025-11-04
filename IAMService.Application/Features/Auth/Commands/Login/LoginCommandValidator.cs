using FluentValidation;

namespace IAMService.Application.Features.Auth.Commands.Login;

/// <summary>
/// Validation for login form.
/// </summary>
/// <seealso cref="FluentValidation.AbstractValidator&lt;IAMService.Application.Features.Auth.Commands.Login.LoginCommand&gt;" />
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandValidator"/> class.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Must(IsValidPassword).WithMessage("Password must include uppercase & lowercase letters and numbers.");
    }

    /// <summary>
    /// Determines whether [is valid password] [the specified password].
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>
    ///   <c>true</c> if [is valid password] [the specified password]; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsValidPassword(string password)
    {
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);

        return hasUpper && hasLower && hasDigit;
    }
}
