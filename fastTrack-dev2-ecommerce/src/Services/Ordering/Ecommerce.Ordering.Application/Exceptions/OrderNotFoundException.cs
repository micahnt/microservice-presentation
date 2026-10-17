using Ecommerce.Shared.Exceptions;

namespace Ecommerce.Ordering.Application.Exceptions;
public class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException(Guid id) : base("Order", id)
    {
    }
}
