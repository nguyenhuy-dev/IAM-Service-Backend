using IAMService.Domain.Entities;

namespace IAMService.Application.Interfaces.AuthenticationServices;

/// <summary>
/// Interface for user token generator.
/// </summary>
public interface IUserTokenGenerator
{
    /// <summary>
    /// Generates the token asynchronous.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<JwtToken> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}
