using IAMService.Application.Interfaces.AccessToken;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace IAMService.Infrastructure.Services.AccessToken
{
    /// <summary>
    /// A service implementation for invalidating (revoking/blacklisting) access tokens using Redis.
    /// It implements the <see cref="IInvalidationService"/> interface.
    /// </summary>
    public class InvalidationAccessTokenService : IInvalidationService
    {
        private readonly IDatabase _redisDb;
        private readonly ITokenDecoderService _tokenDecoderService;
        private const string DenyListPrefix = "denylist:"; // Changed to include a separator for clarity

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidationAccessTokenService"/> class.
        /// </summary>
        /// <param name="redisDb">The Redis database instance used for storing the token deny list.</param>
        /// <param name="tokenDecoderService">The service used to extract information (JTI, expiry) from the token.</param>
        public InvalidationAccessTokenService(IDatabase redisDb, ITokenDecoderService tokenDecoderService)
        {
            _redisDb = redisDb;
            _tokenDecoderService = tokenDecoderService;
        }

        /// <summary>
        /// Asynchronously invalidates the specified access token by adding its identifier to the Redis deny list.
        /// The key is set to expire at the token's original expiration time, ensuring automatic cleanup.
        /// </summary>
        /// <param name="accessTokenValue">The raw string value of the access token to be invalidated.</param>
        /// <returns>A task that represents the asynchronous invalidation operation.</returns>
        public async Task InvalidationAccessTokenAsync(string accessTokenValue)
        {
            // Extract the unique token ID (JTI) and its remaining valid time
            string tokenIdentifier = _tokenDecoderService.GetTokenIdentifier(accessTokenValue);
            TimeSpan expiryTime = _tokenDecoderService.GetRemainingExpirationTime(accessTokenValue);

            // If the token ID is null/empty or the token is already expired, there's nothing to invalidate
            if (string.IsNullOrEmpty(tokenIdentifier) || expiryTime <= TimeSpan.Zero)
            {
                return;
            }

            // Construct the unique Redis key for the token
            string redisKey = DenyListPrefix + tokenIdentifier;

            // Add the token identifier to Redis with the remaining expiry time
            await _redisDb.StringSetAsync(
                key: redisKey,
                value: "revoked", // The actual value doesn't matter, only the key's existence
                expiry: expiryTime,
                when: When.Always
            );
        }

        /// <summary>
        /// Asynchronously checks if a token, identified by its unique identifier (JTI), has been explicitly invalidated
        /// by checking for its presence in the Redis deny list.
        /// </summary>
        /// <param name="tokenIdentifier">The unique identifier (JTI) of the token to check.</param>
        /// <returns>A task that represents the asynchronous check operation. The task result is <c>true</c> if the token is invalidated; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsTokenInvalidatedAsync(string tokenIdentifier)
        {
            // If the identifier is missing, it cannot be in the denylist (and should be treated as valid)
            if (string.IsNullOrEmpty(tokenIdentifier))
            {
                return false;
            }

            // Construct the Redis key
            string redisKey = DenyListPrefix + tokenIdentifier;

            // Check if the key exists in the database
            bool exists = await _redisDb.KeyExistsAsync(redisKey);

            return exists;
        }
    }
}