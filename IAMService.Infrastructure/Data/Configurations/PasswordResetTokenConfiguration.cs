using IAMService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configures the database schema and relationships for the <see cref="PasswordResetToken"/> entity 
    /// using Fluent API conventions.
    /// </summary>
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        /// <summary>
        /// Configures the entity of type <see cref="PasswordResetToken"/>.
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.Property(t => t.ExpiresAt)
                .IsRequired();

            builder.Property(t => t.IsUsed)
                .HasDefaultValue(false);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.Token)
                .IsUnique();

            builder.HasIndex(t => new { t.UserId, t.ExpiresAt });
        }
    }
}