namespace Ecommerce.Catalog.Api.Presentation.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product p) =>
        new(p.Id, p.Name, p.Category, p.Description, p.ImageFile, p.Price, p.Stock);

    public static Product ToEntity(this CreateProductRequest r) => new()
    {
        Name = r.Name, Category = r.Category, Description = r.Description,
        ImageFile = r.ImageFile, Price = r.Price, Stock = r.Stock,
    };

    public static void Apply(this UpdateProductRequest r, Product p)
    {
        p.Name = r.Name;
        p.Category = r.Category;
        p.Description = r.Description;
        p.ImageFile = r.ImageFile;
        p.Price = r.Price;
        p.Stock = r.Stock;
    }
}
