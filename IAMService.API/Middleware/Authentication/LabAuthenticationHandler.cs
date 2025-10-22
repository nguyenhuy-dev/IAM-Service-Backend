using IAMService.Application.Interfaces;

namespace IAMService.API.Middleware.Authentication;

/// <summary>
/// Lab authentication handler implementation.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationHandler&lt;IAMService.API.Middleware.Authentication.LabAuthenticationSchemeOptions&gt;" />
public class LabAuthenticationHandler : AuthenticationHandler<LabAuthenticationSchemeOptions>
{
    /// <summary>
    /// The authentication repository
    /// </summary>
    private readonly IAuthRepository _authRepository;
    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="options">The options.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="encoder">The encoder.</param>
    public LabAuthenticationHandler(IAuthRepository authRepository, IOptionsMonitor<LabAuthenticationSchemeOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder) : base(options, loggerFactory, encoder)
    {
        _authRepository = authRepository;
        _logger = loggerFactory.CreateLogger<LabAuthenticationHandler>();
    }

    /// <summary>
    /// Allows derived types to handle authentication.
    /// </summary>
    /// <returns>
    /// The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticateResult" />.
    /// </returns>
    /// <exception cref="System.UnauthorizedAccessException">
    /// Missing Bearer header
    /// or
    /// Invalid or expired token
    /// </exception>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _logger.LogInformation("Handling authentication...");

        var path = Context.Request.Path.Value?.ToLower();
        if (IsPassedPath(path))
            return Task.FromResult(AuthenticateResult.NoResult());

        var bearerHeader = Context.Request.Headers["Bearer"];

        if (bearerHeader.Count == 0)
            throw new UnauthorizedAccessException("Missing Bearer header");

        var tokenValue = bearerHeader[0];
        if (!string.IsNullOrEmpty(tokenValue) && VerifyToken(tokenValue, out ClaimsPrincipal? claimsPrincipal))
        {
            var ticket = new AuthenticationTicket(claimsPrincipal!, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        else
            throw new UnauthorizedAccessException("Invalid or expired token");
    }

    /// <summary>
    /// Determines whether [is passed path] [the specified path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if [is passed path] [the specified path]; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsPassedPath(string? path)
    {
        return path!.StartsWith("/api/login") ||
            !path!.StartsWith("/api");
    }

    /// <summary>
    /// Verifies the token.
    /// </summary>
    /// <param name="tokenValue">The token value.</param>
    /// <param name="claimsPrincipal">The claims principal.</param>
    /// <returns></returns>
    private bool VerifyToken(string tokenValue, out ClaimsPrincipal? claimsPrincipal)
    {
        // Check token validity
        if (!Validate(tokenValue, out claimsPrincipal))
        {
            _logger.LogError("Token validation failed.");
            return false;
        }

        // Check token against database
        if (!ValidateDb(claimsPrincipal))
        {
            _logger.LogError("Token not found in database or is revoked.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates the database.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal.</param>
    /// <returns></returns>
    private bool ValidateDb(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal == null)
            return false;

        var refreshToken = claimsPrincipal.FindFirst("retoken")?.Value;

        if (string.IsNullOrEmpty(refreshToken))
            return false;

        return _authRepository.CheckValidToken(refreshToken).Result;
    }

    /// <summary>
    /// Validates the specified token value.
    /// </summary>
    /// <param name="tokenValue">The token value.</param>
    /// <param name="claimsPrincipal">The claims principal.</param>
    /// <returns></returns>
    private bool Validate(string tokenValue, out ClaimsPrincipal? claimsPrincipal)
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Options.IssuerSigningKey)),
            ValidateIssuer = true,
            ValidIssuer = Options.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = Options.ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            claimsPrincipal = handler.ValidateToken(tokenValue, tokenValidationParameters, out SecurityToken? securityToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            claimsPrincipal = null;

            return false;
        }
    }
}
