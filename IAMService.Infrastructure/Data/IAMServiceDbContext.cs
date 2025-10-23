using IAMService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IAMService.Infrastructure.Data;

/// <summary>
/// Database context for the IAM Service.
/// </summary>
/// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
public class IAMServiceDbContext(DbContextOptions<IAMServiceDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the users.
    /// </summary>
    /// <value>
    /// The users.
    /// </value>
    public DbSet<User> Users { get; set; } = default!;

    /// <summary>
    /// Gets or sets the roles.
    /// </summary>
    /// <value>
    /// The roles.
    /// </value>
    public DbSet<Role> Roles { get; set; } = default!;

    /// <summary>
    /// Gets or sets the privileges.
    /// </summary>
    /// <value>
    /// The privileges.
    /// </value>
    public DbSet<Privilege> Privileges { get; set; } = default!;
    public DbSet<UserToken> UserTokens { get; set; } = default!;

    /// <summary>
    /// Gets or sets the refresh tokens used for persistent user sessions.
    /// </summary>
    /// <value>
    /// The collection of <see cref="RefreshToken"/> entities.
    /// </value>
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    /// <summary>
    /// Gets or sets the password reset tokens used for the forgot password feature.
    /// </summary>
    /// <value>
    /// The collection of <see cref="PasswordResetToken"/> entities.
    /// </value>
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = default!; // Đảm bảo dùng = default! cho tính nhất quán

    /// <summary>
    /// Override this method to further configure the model that was discovered by convention from the entity types
    /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
    /// define extension methods on this object that allow you to configure aspects of the model that are specific
    /// to a given database.</param>
    /// <remarks>
    /// <para>
    /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
    /// then this method will not be run. However, it will still run when creating a compiled model.
    /// </para>
    /// <para>
    /// See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    /// examples.
    /// </para>
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Automatically applies all IEntityTypeConfiguration classes found in the Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Sets the default database schema (common for PostgreSQL/SQL Server environments)
        modelBuilder.HasDefaultSchema("public");
    }
}