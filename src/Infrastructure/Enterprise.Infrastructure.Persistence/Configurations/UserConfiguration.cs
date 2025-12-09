using Enterprise.Core.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enterprise.Infrastructure.Persistence.Configurations;

/// <summary>
/// User entity konfigürasyonu
/// SqlServer ve Oracle destekli
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Tablo adı (schema olmadan - Oracle uyumlu)
        builder.ToTable("USERS");

        // Primary key
        builder.HasKey(u => u.Id);

        // Properties - Oracle IDENTITY veya SqlServer IDENTITY
        builder.Property(u => u.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Username)
            .HasColumnName("USERNAME")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("PASSWORD_HASH")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Email)
            .HasColumnName("EMAIL")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FullName)
            .HasColumnName("FULL_NAME")
            .HasMaxLength(200);

        builder.Property(u => u.Roles)
            .HasColumnName("ROLES")
            .HasMaxLength(500);

        builder.Property(u => u.IsActive)
            .HasColumnName("IS_ACTIVE")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("LAST_LOGIN_AT");

        // Audit properties
        builder.Property(u => u.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasColumnName("CREATED_BY")
            .HasMaxLength(100);

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("UPDATED_AT");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("UPDATED_BY")
            .HasMaxLength(100);

        builder.Property(u => u.IsDeleted)
            .HasColumnName("IS_DELETED")
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .HasColumnName("DELETED_AT");

        builder.Property(u => u.DeletedBy)
            .HasColumnName("DELETED_BY")
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IDX_USERS_USERNAME");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IDX_USERS_EMAIL");

        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IDX_USERS_IS_DELETED");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IDX_USERS_IS_ACTIVE");

        // Navigation property
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Domain events'i ignore et
        builder.Ignore(u => u.DomainEvents);
    }
}


