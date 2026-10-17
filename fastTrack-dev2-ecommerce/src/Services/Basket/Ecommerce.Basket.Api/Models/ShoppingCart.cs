namespace Ecommerce.Basket.Api.Models;

public class ShoppingCart
{
    public Guid UserId { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);

    public ShoppingCart(Guid userId)
    {
        UserId = userId;
    }

    // Required for JSON deserialization
    public ShoppingCart()
    {
    }
}

public class ShoppingCartItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ImageFile { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }   // from Catalog
    public decimal Discount { get; set; }    // from Discount gRPC
    public decimal Price => Math.Max(0, UnitPrice - Discount);
}
