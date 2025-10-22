using IAMService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAMService.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configures the Entity Framework Core mapping for the <see cref="RefreshToken"/> domain entity.
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        /// <summary>
        /// Configures the properties and relationships of the <see cref="RefreshToken"/> entity.
        /// </summary>
        /// <param name="builder">The builder used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // Primary Key Configuration
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Id).ValueGeneratedOnAdd();

            // Relationship to User (Many-to-One)
            builder.HasOne(rt => rt.User)
                    .WithMany() // Assuming the User entity does not have a navigation collection back to RefreshToken
                    .HasForeignKey(rt => rt.UserId)
                    .IsRequired();

            // Self-Referencing Relationship for Token Rotation (One-to-One, Optional)
            builder.HasOne(rt => rt.ReplacedByToken)
                    .WithOne() // A token is only replaced by one new token (or none)
                    .HasForeignKey<RefreshToken>(rt => rt.ReplacedByTokenId)
                    .IsRequired(false) // Allows the foreign key to be null
                    .OnDelete(DeleteBehavior.Restrict); // Prevents cascading delete

            // Property Configurations
            builder.Property(rt => rt.TokenHash)
                   .IsRequired()
                   .HasMaxLength(256);

            // Ensures token hashes are unique, providing a quick lookup mechanism
            builder.HasIndex(rt => rt.TokenHash).IsUnique();

            builder.Property(rt => rt.CreatedAt).IsRequired();
            builder.Property(rt => rt.ExpiresAt).IsRequired();

            // RevokedAt is nullable
            builder.Property(rt => rt.RevokedAt).IsRequired(false);

            // Ignores the computed property IsActive, as it is derived from other properties
            builder.Ignore(rt => rt.IsActive);
        }
    }
}