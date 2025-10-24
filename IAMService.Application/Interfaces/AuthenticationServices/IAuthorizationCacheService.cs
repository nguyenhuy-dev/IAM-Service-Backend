namespace IAMService.Application.Interfaces.AuthenticationServices;

/// <summary>
/// Cache service for authorization policies.
/// </summary>
public interface IAuthorizationCacheService
{
    /// <summary>
    /// Sets the policy.
    /// </summary>
    /// <param name="policyName">Name of the policy.</param>
    /// <param name="policy">The policy.</param>
    /// <param name="timeSpan">The time span.</param>
    void SetPolicy(string policyName, object policy, TimeSpan? timeSpan = null);

    /// <summary>
    /// Tries the get policy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="policyName">Name of the policy.</param>
    /// <param name="policy">The policy.</param>
    /// <returns></returns>
    bool TryGetPolicy<T>(string policyName, out T? policy);
}
