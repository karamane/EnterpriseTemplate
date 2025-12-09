using Enterprise.Core.Domain.Entities.Sample;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enterprise.Infrastructure.Persistence.Configurations;

/// <summary>
/// OrderItem entity EF Core configuration
/// Oracle ve SQL Server uyumlu
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Tablo adı (Oracle uyumlu)
        builder.ToTable("ORDER_ITEMS");

        // Primary key
        builder.HasKey(e => e.Id);

        // ID - Auto-increment
        builder.Property(e => e.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        // OrderId
        builder.Property(e => e.OrderId)
            .HasColumnName("ORDER_ID")
            .IsRequired();

        // ProductId
        builder.Property(e => e.ProductId)
            .HasColumnName("PRODUCT_ID")
            .IsRequired();

        // ProductName
        builder.Property(e => e.ProductName)
            .HasColumnName("PRODUCT_NAME")
            .HasMaxLength(200)
            .IsRequired();

        // Quantity
        builder.Property(e => e.Quantity)
            .HasColumnName("QUANTITY")
            .IsRequired();

        // UnitPrice
        builder.Property(e => e.UnitPrice)
            .HasColumnName("UNIT_PRICE")
            .HasPrecision(18, 2)
            .IsRequired();

        // CreatedAt
        builder.Property(e => e.CreatedAt)
            .HasColumnName("CREATED_AT")
            .IsRequired();

        // Ignore computed property
        builder.Ignore(e => e.TotalPrice);

        // Indexes
        builder.HasIndex(e => e.OrderId)
            .HasDatabaseName("IDX_ORDER_ITEMS_ORDER");

        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("IDX_ORDER_ITEMS_PRODUCT");
    }
}


