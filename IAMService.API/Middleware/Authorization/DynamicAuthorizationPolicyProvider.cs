using IAMService.Application.Exceptions;
namespace IAMService.API.Middleware.Authorization
{
    /// <summary>
    ///     Dynamic authorization policy provider.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authorization.DefaultAuthorizationPolicyProvider" />
    public class DynamicAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        /// <summary>
        ///     The options
        /// </summary>
        private readonly AuthorizationOptions _options;

        /// <summary>
        ///     The scope factory
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicAuthorizationPolicyProvider" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="scopeFactory">The scope factory.</param>
        public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IServiceScopeFactory scopeFactory) : base(options)
        {
            _options = options.Value;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        ///     Gets a <see cref="T:Microsoft.AspNetCore.Authorization.AuthorizationPolicy" /> from the given
        ///     <paramref name="policyName" />
        /// </summary>
        /// <param name="policyName">The policy name to retrieve.</param>
        /// <returns>
        ///     The named <see cref="T:Microsoft.AspNetCore.Authorization.AuthorizationPolicy" />.
        /// </returns>
        /// <exception cref="IAMService.Application.Exceptions.ForbiddenAccessException">
        ///     Invalid policy format.
        ///     or
        ///     Role does not exist.
        ///     or
        ///     Privilege does not exist for the specified role.
        ///     or
        ///     Failed to create authorization policy.
        /// </exception>
        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Check registered policy
            if (_options.GetPolicy(policyName) is { } existingPolicy)
                return existingPolicy;

            // Check cached policy
            using var scope = _scopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IAuthorizationCacheService>();
            if (cache.TryGetPolicy(policyName, out AuthorizationPolicy? cachedPolicy) && cachedPolicy != null)
                return cachedPolicy;

            if (string.IsNullOrEmpty(policyName))
                throw new ForbiddenAccessException("Invalid policy format.");

            var dbContext = scope.ServiceProvider.GetRequiredService<IAMServiceDbContext>();

            var privilege = await dbContext.Privileges.Include(p => p.Roles).FirstOrDefaultAsync(p => p.PrivilegeName == policyName);
            if (privilege == null)
                throw new ForbiddenAccessException("Privilege does not exist.");

            var roleCodes = privilege.Roles.Select(r => r.RoleName);
            AuthorizationPolicy policy;
            try
            {
                policy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(context =>
                        roleCodes.Any(rc => context.User.IsInRole(rc))
                    )
                    .Build();
            }
            catch (Exception ex)
            {
                throw new ForbiddenAccessException("Failed to create authorization policy.", ex);
            }

            // Cache the policy for future requests
            cache.SetPolicy(policyName, policy, TimeSpan.FromMinutes(1));

            return policy;
        }
    }
}
