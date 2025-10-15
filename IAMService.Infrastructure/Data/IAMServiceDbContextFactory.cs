using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IAMService.Infrastructure.Data;

/// <summary>
/// Factory for generating migration files.
/// </summary>
/// <seealso cref="Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory&lt;IAMService.Infrastructure.Data.IAMServiceDbContext&gt;" />
public class IAMServiceDbContextFactory : IDesignTimeDbContextFactory<IAMServiceDbContext>
{
    /// <summary>
    /// Creates a new instance of a derived context.
    /// </summary>
    /// <param name="args">Arguments provided by the design-time service.</param>
    /// <returns>
    /// An instance of <typeparamref name="TContext" />.
    /// </returns>
    public IAMServiceDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../IAMService.API"))
            .AddUserSecrets("6c81c765-f24e-494c-a510-fd27848536f1")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<IAMServiceDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new IAMServiceDbContext(optionsBuilder.Options);
    }
}
