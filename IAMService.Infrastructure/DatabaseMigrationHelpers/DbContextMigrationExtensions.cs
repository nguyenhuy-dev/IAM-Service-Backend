using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IAMService.Infrastructure.DatabaseMigrationHelpers;

/// <summary>
/// Add migration services.
/// </summary>
public static class DbContextMigrationExtensions
{
    /// <summary>
    /// Migrates the database context asynchronous.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="app">The application.</param>
    /// <param name="postMigration">The post migration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static async Task<IApplicationBuilder> MigrateDbContextAsync<TContext>(this IApplicationBuilder app, Func<DatabaseFacade, CancellationToken?, Task>? postMigration = null, CancellationToken cancellationToken = default) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();
        var context = services.GetService<TContext>();
        if (context is not null)
        {
            try
            {
                var strategy = context.Database.CreateExecutionStrategy();

                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                await strategy.ExecuteAsync(async () =>
                {
                    await context.Database.MigrateAsync(cancellationToken);
                });
                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);

                if (postMigration != null)
                {
                    try
                    {
                        logger.LogInformation("Invoking postMigration function...");

                        await postMigration.Invoke(context.Database, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error invoking postMigration function");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
            }
        }

        return app;
    }
}
