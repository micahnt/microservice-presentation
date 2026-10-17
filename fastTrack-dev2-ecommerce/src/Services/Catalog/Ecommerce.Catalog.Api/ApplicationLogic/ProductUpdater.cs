namespace Ecommerce.Catalog.Api.ApplicationLogic;

public interface IProductUpdater
{
    Task Update(Guid id, UpdateProductRequest request);
    Task DecrementStock(IReadOnlyCollection<(Guid ProductId, int Quantity)> items);
    Task IncrementStock(IReadOnlyCollection<(Guid ProductId, int Quantity)> items);
}

public class ProductUpdater(IProductRepository repository, ILogger<ProductUpdater> log) : IProductUpdater
{
    public async Task Update(Guid id, UpdateProductRequest request)
    {
        var product = await repository.GetForUpdate(id) ?? throw new ProductNotFoundException(id);
        request.Apply(product);
        await repository.SaveChanges();
    }

    public async Task DecrementStock(IReadOnlyCollection<(Guid ProductId, int Quantity)> items)
    {
        var products = (await repository.GetForUpdate(items.Select(i => i.ProductId)))
            .ToDictionary(p => p.Id);

        foreach (var (productId, quantity) in items)
        {
            if (!products.TryGetValue(productId, out var product))
            {
                log.LogWarning("Stock not updated: product {ProductId} no longer exists", productId);
                continue;
            }
            product.Stock = Math.Max(0, product.Stock - quantity);
        }

        await repository.SaveChanges();
    }

    // Gives stock back for a cancelled order. Called once per OrderCancelledEvent: the consumer inbox
    // (UseEntityFrameworkOutbox on every endpoint) drops redelivered messages.
    public async Task IncrementStock(IReadOnlyCollection<(Guid ProductId, int Quantity)> items)
    {
        var products = (await repository.GetForUpdate(items.Select(i => i.ProductId)))
            .ToDictionary(p => p.Id);

        foreach (var (productId, quantity) in items)
        {
            if (!products.TryGetValue(productId, out var product))
            {
                log.LogWarning("Stock not returned: product {ProductId} no longer exists", productId);
                continue;
            }
            product.Stock += quantity;
        }

        await repository.SaveChanges();
    }
}
