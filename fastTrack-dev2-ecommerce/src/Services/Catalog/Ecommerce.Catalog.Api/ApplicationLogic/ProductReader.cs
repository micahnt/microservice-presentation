namespace Ecommerce.Catalog.Api.ApplicationLogic;

public interface IProductReader
{
    Task<PaginatedResult<Product>> Search(ProductQuery query);
    Task<Product> GetById(Guid id);
    Task<List<Product>> GetByCategory(string category);
    Task<List<string>> GetCategories();
}

public class ProductReader(IProductRepository repository) : IProductReader
{
    public Task<PaginatedResult<Product>> Search(ProductQuery query) => repository.Search(query);

    public async Task<Product> GetById(Guid id) =>
        await repository.GetById(id) ?? throw new ProductNotFoundException(id);

    public Task<List<Product>> GetByCategory(string category) => repository.GetByCategory(category);

    public Task<List<string>> GetCategories() => repository.GetCategories();
}
