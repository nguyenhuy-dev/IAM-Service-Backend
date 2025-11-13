namespace IAMService.Application.Interfaces.AccessToken
{
    /// <summary>
    ///     Defines a service responsible for managing the invalidation (revocation) of access tokens.
    ///     This is typically used to blacklist tokens before their natural expiration time.
    /// </summary>
    public interface IInvalidationService
    {
        /// <summary>
        ///     Asynchronously invalidates the specified access token, typically by adding its identifier
        ///     to a revocation list or blacklist.
        /// </summary>
        /// <param name="accessTokenValue">The raw string value of the access token to be invalidated.</param>
        /// <returns>A task that represents the asynchronous invalidation operation.</returns>
        Task InvalidationAccessTokenAsync(string accessTokenValue);

        /// <summary>
        ///     Asynchronously checks if a token, identified by its unique identifier (JTI), has been explicitly invalidated.
        /// </summary>
        /// <param name="tokenIdentifier">The unique identifier (JTI) of the token to check.</param>
        /// <returns>
        ///     A task that represents the asynchronous check operation. The task result is <c>true</c> if the token is
        ///     invalidated; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> IsTokenInvalidatedAsync(string tokenIdentifier);
    }
}
