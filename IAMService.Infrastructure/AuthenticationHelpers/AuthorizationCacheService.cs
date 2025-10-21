using IAMService.Application.Interfaces.AuthenticationServices;
using Microsoft.Extensions.Caching.Memory;

namespace IAMService.Infrastructure.AuthenticationHelpers;

/// <summary>
/// Authorization cache service implementation.
/// </summary>
public class AuthorizationCacheService : IAuthorizationCacheService
{
    /// <summary>
    /// The cache
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationCacheService"/> class.
    /// </summary>
    /// <param name="cache">The cache.</param>
    public AuthorizationCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Sets the policy.
    /// </summary>
    /// <param name="policyName">Name of the policy.</param>
    /// <param name="policy">The policy.</param>
    /// <param name="timeSpan">The time span.</param>
    public void SetPolicy(string policyName, object policy, TimeSpan? timeSpan = null)
    {
        timeSpan ??= TimeSpan.FromHours(5);
        _cache.Set(policyName, policy, timeSpan.Value);
    }

    /// <summary>
    /// Tries the get policy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="policyName">Name of the policy.</param>
    /// <param name="policy">The policy.</param>
    /// <returns></returns>
    public bool TryGetPolicy<T>(string policyName, out T? policy)
    {
        if (_cache.TryGetValue(policyName, out var cachedObj) && cachedObj is T casted)
        {
            policy = casted;
            return true;
        }

        policy = default;
        return false;
    }
}
