namespace Ecommerce.Basket.Api.Presentation.Mappers;

public static class BasketMapper
{
    public static BasketDto ToDto(this ShoppingCart cart) =>
        new(cart.UserId, cart.Items.Select(i => i.ToDto()).ToList(), cart.TotalPrice);

    public static BasketItemDto ToDto(this ShoppingCartItem i) =>
        new(i.ProductId, i.ProductName, i.ImageFile, i.Quantity, i.UnitPrice, i.Discount, i.Price);
}
