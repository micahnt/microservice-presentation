using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Ordering.Infrastructure.Data.Configurations;

// No FK to a Products table: the line stores a snapshot (id, name, price) of the Catalog product.
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id).HasConversion(
                                   orderItemId => orderItemId.Value,
                                   dbId => OrderItemId.Of(dbId));

        builder.Property(oi => oi.ProductId).HasConversion(
                                   productId => productId.Value,
                                   dbId => ProductId.Of(dbId));

        builder.Property(oi => oi.ProductName).HasMaxLength(200).IsRequired();

        builder.Property(oi => oi.Quantity).IsRequired();

        builder.Property(oi => oi.Price).HasPrecision(18, 2).IsRequired();
    }
}
