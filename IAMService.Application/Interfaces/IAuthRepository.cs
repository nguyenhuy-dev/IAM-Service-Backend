namespace IAMService.Application.Interfaces;

/// <summary>
/// Interface for authentication repository.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Checks the valid token.
    /// </summary>
    /// <param name="tokenValue">The token value.</param>
    /// <returns></returns>
    Task<bool> CheckValidToken(string tokenValue);
}
