namespace Ecommerce.Catalog.Api.ApplicationLogic;

public interface IProductDeleter
{
    Task Delete(Guid id);
}

public class ProductDeleter(IProductRepository repository) : IProductDeleter
{
    public async Task Delete(Guid id)
    {
        var product = await repository.GetForUpdate(id) ?? throw new ProductNotFoundException(id);
        repository.Remove(product);
        await repository.SaveChanges();
    }
}
