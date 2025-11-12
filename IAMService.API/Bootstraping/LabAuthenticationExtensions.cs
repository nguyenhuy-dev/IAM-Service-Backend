using IAMService.API.Middleware.Authentication;
namespace IAMService.API.Bootstraping
{
    /// <summary>
    ///     Lab authentication extensions.
    /// </summary>
    public static class LabAuthenticationExtensions
    {
        /// <summary>
        ///     Adds the lab token.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddLabToken(this AuthenticationBuilder builder, Action<LabAuthenticationSchemeOptions> options)
        {
            builder.AddScheme<LabAuthenticationSchemeOptions, LabAuthenticationHandler>("Token", options);
            builder.Services.AddScoped<IUserTokenGenerator, UserTokenGenerator>();

            return builder;
        }
    }
}
