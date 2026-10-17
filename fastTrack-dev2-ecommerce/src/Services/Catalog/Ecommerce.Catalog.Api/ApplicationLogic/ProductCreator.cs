namespace Ecommerce.Catalog.Api.ApplicationLogic;

public interface IProductCreator
{
    Task<Product> Create(Product product);
}

public class ProductCreator(IProductRepository repository) : IProductCreator
{
    public async Task<Product> Create(Product product)
    {
        product.Id = Guid.NewGuid();
        repository.Add(product);
        await repository.SaveChanges();
        return product;
    }
}
