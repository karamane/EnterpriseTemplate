using Enterprise.Core.Domain.Entities.Sample;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enterprise.Infrastructure.Persistence.Configurations;

/// <summary>
/// Order entity EF Core configuration
/// Oracle ve SQL Server uyumlu
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Tablo adı (Oracle uyumlu)
        builder.ToTable("ORDERS");

        // Primary key
        builder.HasKey(e => e.Id);

        // ID - Auto-increment
        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        // CustomerId
        builder.Property(e => e.CustomerId)
            .HasColumnName("CUSTOMER_ID")
            .IsRequired();

        // TotalAmount
        builder.Property(e => e.TotalAmount)
            .HasColumnName("TOTAL_AMOUNT")
            .HasPrecision(18, 2)
            .IsRequired();

        // Status
        builder.Property(e => e.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(50)
            .IsRequired();

        // Notes
        builder.Property(e => e.Notes)
            .HasColumnName("NOTES")
            .HasMaxLength(500);

        // OrderDate
        builder.Property(e => e.OrderDate)
            .HasColumnName("ORDER_DATE")
            .IsRequired();

        // Audit fields
        builder.Property(e => e.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasColumnName("CREATED_BY")
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UPDATED_AT");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UPDATED_BY")
            .HasMaxLength(100);

        // Soft delete fields
        builder.Property(e => e.IsDeleted)
            .HasColumnName("IS_DELETED")
            .IsRequired();

        builder.Property(e => e.DeletedAt)
            .HasColumnName("DELETED_AT");

        builder.Property(e => e.DeletedBy)
            .HasColumnName("DELETED_BY")
            .HasMaxLength(100);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("IDX_ORDERS_CUSTOMER");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IDX_ORDERS_STATUS");

        builder.HasIndex(e => e.OrderDate)
            .HasDatabaseName("IDX_ORDERS_DATE");

        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName("IDX_ORDERS_IS_DELETED");
    }
}


