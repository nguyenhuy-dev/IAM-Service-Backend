using System.Text.Json;

namespace IAMService.API.Middleware.Authentication;

/// <summary>
/// Lab authentication handler implementation.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationHandler&lt;IAMService.API.Middleware.Authentication.LabAuthenticationSchemeOptions&gt;" />
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
    /// Initializes a new instance of the <see cref="LabAuthenticationHandler" /> class.
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
    /// <exception cref="System.UnauthorizedAccessException">Missing Bearer header
    /// or
    /// Invalid or expired token</exception>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _logger.LogInformation("Handling authentication...");

        var path = Context.Request.Path.Value?.ToLower();
        if (IsPassedPath(path))
        {
            _logger.LogInformation("No authentication with passing path.");
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var authorHeader = Context.Request.Headers.Authorization;

        if (authorHeader.Count == 0)
            return Task.FromResult(AuthenticateResult.Fail("Missing Bearer header"));

        var tokenValue = authorHeader[0]?.Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(tokenValue) && VerifyToken(tokenValue, out ClaimsPrincipal? claimsPrincipal))
        {
            var ticket = new AuthenticationTicket(claimsPrincipal!, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        else
            return Task.FromResult(AuthenticateResult.Fail("Verify token unsuccessfully."));
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
        if (!ValidateDb(tokenValue))
        {
            _logger.LogError("Token not found in database or is revoked.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates the database.
    /// </summary>
    /// <param name="tokenValue">The token value.</param>
    /// <returns></returns>
    private bool ValidateDb(string tokenValue) => _authRepository.CheckValidToken(tokenValue).Result;

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

    /// <summary>
    /// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
    /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
    /// changing the 401 result to 302 of a login page or external sign-in location.)
    /// </summary>
    /// <param name="properties"></param>
    /// <returns>
    /// A Task.
    /// </returns>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (!Context.Response.HasStarted)
        {
            Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            Context.Response.ContentType = "application/json";

            var result = new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Unauthorized access"
            };

            var json = JsonSerializer.Serialize(result);
            return Context.Response.WriteAsync(json);
        }

        return Task.CompletedTask;
    }
}
