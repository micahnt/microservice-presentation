using Ecommerce.Discount.Grpc;

namespace Ecommerce.Basket.Api.ApplicationLogic;

public interface IBasketStorer
{
    Task<ShoppingCart> Store(IEnumerable<StoreBasketItem> items);
}

// Recomputes every line from Catalog's current price and Discount's coupon on each store,
// so a discount is never applied twice and the client can't choose its own price.
public class BasketStorer(
    IBasketRepository repository,
    ICatalogService catalog,
    DiscountProtoService.DiscountProtoServiceClient discount,
    ICurrentUser currentUser) : IBasketStorer
{
    public async Task<ShoppingCart> Store(IEnumerable<StoreBasketItem> items)
    {
        var cart = new ShoppingCart(currentUser.UserId);

        var lines = items
            .GroupBy(i => i.ProductId)
            .Select(g => (ProductId: g.Key, Quantity: g.Sum(i => i.Quantity)));

        foreach (var (productId, quantity) in lines)
        {
            var product = await catalog.GetProduct(productId);                                  // HTTP (Refit)
            if (product.Stock < quantity)
                throw new BadRequestException($"Only {product.Stock} left of {product.Name}.");

            var coupon = await discount.GetDiscountAsync(
                new GetDiscountRequest { ProductName = product.Name });                        // gRPC

            cart.Items.Add(new ShoppingCartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageFile = product.ImageFile,
                Quantity = quantity,
                UnitPrice = product.Price,
                Discount = coupon.Amount,
            });
        }

        await repository.Save(cart);
        return cart;
    }
}
