using Enterprise.Core.Domain.Entities.Sample;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enterprise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Customer entity konfigürasyonu
/// EF Core Fluent API kullanımı örneği
/// SqlServer ve Oracle destekli
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Tablo adı (schema olmadan - Oracle uyumlu)
        builder.ToTable("CUSTOMERS");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties - Oracle IDENTITY veya SqlServer IDENTITY
        builder.Property(c => c.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.FirstName)
            .HasColumnName("FIRST_NAME")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .HasColumnName("LAST_NAME")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .HasColumnName("EMAIL")
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("PHONE_NUMBER")
            .HasMaxLength(20);

        builder.Property(c => c.IsActive)
            .HasColumnName("IS_ACTIVE")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.RegisteredAt)
            .HasColumnName("REGISTERED_AT")
            .IsRequired();

        // Audit properties
        builder.Property(c => c.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasColumnName("CREATED_BY")
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("UPDATED_AT");

        builder.Property(c => c.UpdatedBy)
            .HasColumnName("UPDATED_BY")
            .HasMaxLength(100);

        builder.Property(c => c.IsDeleted)
            .HasColumnName("IS_DELETED");

        builder.Property(c => c.DeletedAt)
            .HasColumnName("DELETED_AT");

        builder.Property(c => c.DeletedBy)
            .HasColumnName("DELETED_BY")
            .HasMaxLength(100);

        // Indexes (filter olmadan - Oracle uyumlu)
        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasDatabaseName("IDX_CUSTOMERS_EMAIL");

        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("IDX_CUSTOMERS_IS_DELETED");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IDX_CUSTOMERS_IS_ACTIVE");

        // Domain events'i ignore et
        builder.Ignore(c => c.DomainEvents);
    }
}

