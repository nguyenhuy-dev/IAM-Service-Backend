namespace IAMService.API.Middleware.Authentication;

/// <summary>
/// Lab authentication scheme options.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions" />
public class LabAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the issuer signing key.
    /// </summary>
    /// <value>
    /// The issuer signing key.
    /// </value>
    public string IssuerSigningKey { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the valid issuer.
    /// </summary>
    /// <value>
    /// The valid issuer.
    /// </value>
    public string ValidIssuer { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the valid audience.
    /// </summary>
    /// <value>
    /// The valid audience.
    /// </value>
    public string ValidAudience { get; set; } = string.Empty;
}
