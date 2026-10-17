using Ecommerce.Shared.Exceptions;

namespace Ecommerce.Catalog.Api.Exceptions;

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(Guid Id) : base("Product", Id)
    {
    }
}
