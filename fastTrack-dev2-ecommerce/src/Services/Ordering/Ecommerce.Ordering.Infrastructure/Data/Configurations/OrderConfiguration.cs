using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Ordering.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasConversion(
                        orderId => orderId.Value,
                        dbId => OrderId.Of(dbId));

        builder.HasOne<Customer>()
          .WithMany()
          .HasForeignKey(o => o.CustomerId)
          .IsRequired();

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId);

        builder.ComplexProperty(
            o => o.OrderName, nameBuilder =>
            {
                nameBuilder.Property(n => n.Value)
                    .HasColumnName(nameof(Order.OrderName))
                    .HasMaxLength(100)
                    .IsRequired();
            });

        builder.ComplexProperty(o => o.ShippingAddress, ConfigureAddress);
        builder.ComplexProperty(o => o.BillingAddress, ConfigureAddress);

        builder.ComplexProperty(
               o => o.Payment, paymentBuilder =>
               {
                   paymentBuilder.Property(p => p.Method).IsRequired();
                   paymentBuilder.Property(p => p.TransactionId).HasMaxLength(50);
               });

        builder.Property(o => o.Status)
            .HasDefaultValue(OrderStatus.Pending)
            .HasConversion(
                s => s.ToString(),
                dbStatus => (OrderStatus)Enum.Parse(typeof(OrderStatus), dbStatus))
            .HasMaxLength(20);

        // One order per checkout event: makes the consumer idempotent even across redeliveries.
        builder.HasIndex(o => o.CheckoutId).IsUnique();

        builder.Property(o => o.TotalPrice).HasPrecision(18, 2);
    }

    private static void ConfigureAddress(ComplexPropertyBuilder<Address> addressBuilder)
    {
        addressBuilder.Property(a => a.FirstName).HasMaxLength(50).IsRequired();
        addressBuilder.Property(a => a.LastName).HasMaxLength(50).IsRequired();
        addressBuilder.Property(a => a.EmailAddress).HasMaxLength(255);
        addressBuilder.Property(a => a.AddressLine).HasMaxLength(180).IsRequired();
        addressBuilder.Property(a => a.Country).HasMaxLength(50);
        addressBuilder.Property(a => a.State).HasMaxLength(50);
        addressBuilder.Property(a => a.ZipCode).HasMaxLength(10).IsRequired();
    }
}
