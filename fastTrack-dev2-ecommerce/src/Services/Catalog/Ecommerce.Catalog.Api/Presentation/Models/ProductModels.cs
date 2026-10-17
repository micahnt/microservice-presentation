namespace Ecommerce.Catalog.Api.Presentation.Models;

public record ProductDto(Guid Id, string Name, List<string> Category, string Description, string ImageFile, decimal Price, int Stock);

public record ProductQuery(int PageIndex = 0, int PageSize = 12, string? Category = null, string? Search = null);

public record CreateProductRequest(string Name, List<string> Category, string Description, string ImageFile, decimal Price, int Stock);

public record UpdateProductRequest(string Name, List<string> Category, string Description, string ImageFile, decimal Price, int Stock);
