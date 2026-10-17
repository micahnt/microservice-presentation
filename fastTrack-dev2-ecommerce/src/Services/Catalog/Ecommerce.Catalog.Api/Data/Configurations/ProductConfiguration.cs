using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Catalog.Api.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ImageFile).HasMaxLength(200);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.HasIndex(x => x.Category).HasMethod("gin"); // fast "category contains"
    }
}
