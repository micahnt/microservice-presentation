namespace Ecommerce.Ordering.Api.Models;

public record OrdersQuery(int PageIndex = 0, int PageSize = 10, string? Name = null);

public record UpdateOrderStatusRequest(OrderStatus Status);
