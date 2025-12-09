using Enterprise.Core.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enterprise.Infrastructure.Persistence.Configurations;

/// <summary>
/// RefreshToken entity konfigürasyonu
/// SqlServer ve Oracle destekli
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Tablo adı (schema olmadan - Oracle uyumlu)
        builder.ToTable("REFRESH_TOKENS");

        // Primary key
        builder.HasKey(rt => rt.Id);

        // Properties - Oracle IDENTITY veya SqlServer IDENTITY
        builder.Property(rt => rt.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.UserId)
            .HasColumnName("USER_ID")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("TOKEN")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("EXPIRES_AT")
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasColumnName("CREATED_BY_IP")
            .HasMaxLength(50);

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("REVOKED_AT");

        builder.Property(rt => rt.RevokedByIp)
            .HasColumnName("REVOKED_BY_IP")
            .HasMaxLength(50);

        builder.Property(rt => rt.ReplacedByToken)
            .HasColumnName("REPLACED_BY_TOKEN")
            .HasMaxLength(500);

        builder.Property(rt => rt.RevokedReason)
            .HasColumnName("REVOKED_REASON")
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IDX_REFRESH_TOKENS_TOKEN");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IDX_REFRESH_TOKENS_USER_ID");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IDX_REFRESH_TOKENS_EXPIRES_AT");

        // Computed properties'i ignore et
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsActive);

        // Domain events'i ignore et
        builder.Ignore(rt => rt.DomainEvents);
    }
}


