namespace Ecommerce.Catalog.Api.Data;

public interface IProductRepository
{
    Task<Product?> GetById(Guid id);
    Task<PaginatedResult<Product>> Search(ProductQuery query);
    Task<List<Product>> GetByCategory(string category);
    Task<List<string>> GetCategories();
    Task<Product?> GetForUpdate(Guid id);
    Task<List<Product>> GetForUpdate(IEnumerable<Guid> ids);
    void Add(Product product);
    void Remove(Product product);
    Task SaveChanges();
}

public class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public Task<Product?> GetById(Guid id) =>
        db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);

    public async Task<PaginatedResult<Product>> Search(ProductQuery q)
    {
        var query = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q.Category)) query = query.Where(p => p.Category.Contains(q.Category));
        if (!string.IsNullOrWhiteSpace(q.Search)) query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q.Search}%"));

        var total = await query.LongCountAsync();
        var items = await query.OrderBy(p => p.Name)
            .Skip(q.PageIndex * q.PageSize).Take(q.PageSize).ToListAsync();
        return new PaginatedResult<Product>(q.PageIndex, q.PageSize, total, items);
    }

    public Task<List<Product>> GetByCategory(string category) =>
        db.Products.AsNoTracking().Where(p => p.Category.Contains(category)).OrderBy(p => p.Name).ToListAsync();

    // EF Core 8 can't translate SelectMany over a text[] column, so the arrays are flattened in memory.
    public async Task<List<string>> GetCategories() =>
        (await db.Products.AsNoTracking().Select(p => p.Category).ToListAsync())
            .SelectMany(c => c).Distinct().Order().ToList();

    public Task<Product?> GetForUpdate(Guid id) => db.Products.SingleOrDefaultAsync(p => p.Id == id);

    public Task<List<Product>> GetForUpdate(IEnumerable<Guid> ids) =>
        db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();

    public void Add(Product product) => db.Products.Add(product);
    public void Remove(Product product) => db.Products.Remove(product);
    public Task SaveChanges() => db.SaveChangesAsync();
}
