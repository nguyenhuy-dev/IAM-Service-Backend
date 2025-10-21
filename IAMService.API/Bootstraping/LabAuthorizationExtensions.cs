namespace IAMService.API.Bootstraping;

/// <summary>
/// Lab authorization extensions.
/// </summary>
public static class LabAuthorizationExtensions
{
    /// <summary>
    /// Adds the lab authorization.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns></returns>
    public static IServiceCollection AddLabAuthorization(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();
        services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();

        services.AddAuthorization();

        return services;
    }
}
