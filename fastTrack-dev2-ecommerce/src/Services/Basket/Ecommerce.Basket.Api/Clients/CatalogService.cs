using System.Net;
using Refit;

namespace Ecommerce.Basket.Api.Clients;

public record CatalogProduct(Guid Id, string Name, string ImageFile, decimal Price, int Stock);

// Refit client. GET /products/{id} is anonymous, so no token is forwarded.
public interface ICatalogClient
{
    [Get("/products/{id}")]
    Task<CatalogProduct> GetProduct(Guid id);
}

public interface ICatalogService
{
    Task<CatalogProduct> GetProduct(Guid id);
}

// Keeps Refit out of the use cases: a 404 from Catalog becomes NotFoundException.
public class CatalogService(ICatalogClient client) : ICatalogService
{
    public async Task<CatalogProduct> GetProduct(Guid id)
    {
        try
        {
            return await client.GetProduct(id);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException("Product", id);
        }
    }
}
