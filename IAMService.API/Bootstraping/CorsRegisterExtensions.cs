namespace IAMService.API.Bootstraping
{
    /// <summary>
    ///     Cors extensions for Program.
    /// </summary>
    public static class CorsRegisterExtensions
    {
        /// <summary>
        ///     Adds the cors lab.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="namePolicy">The name policy.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static IServiceCollection AddCorsLab(this IServiceCollection services, string namePolicy, string[] args)
        {
            return services.AddCors(options =>
            {
                options.AddPolicy(namePolicy, policy =>
                {
                    policy
                        .WithOrigins(args)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }
    }
}
