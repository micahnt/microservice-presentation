namespace Ecommerce.Ordering.Domain.Models;

// ProductName and Price are a snapshot taken at checkout: the order keeps the price it was bought at.
public class OrderItem : Entity<OrderItemId>
{
    internal OrderItem(OrderId orderId, ProductId productId, string productName, int quantity, decimal price)
    {
        Id = OrderItemId.Of(Guid.NewGuid());
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    public OrderId OrderId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public int Quantity { get; private set; } = default!;
    public decimal Price { get; private set; } = default!;
}
