namespace Ecommerce.Basket.Api.Presentation.Models;

public record BasketDto(Guid UserId, List<BasketItemDto> Items, decimal TotalPrice);

public record BasketItemDto(Guid ProductId, string ProductName, string ImageFile, int Quantity,
    decimal UnitPrice, decimal Discount, decimal Price);

// The client sends only product ids and quantities; prices are always computed server-side.
public record StoreBasketRequest(List<StoreBasketItem> Items);

public record StoreBasketItem(Guid ProductId, int Quantity);

public enum PaymentMethod
{
    CashOnDelivery = 1,
    FakeCard = 2,
}

// No card data: the checkout page may show a fake card form, but never sends it.
public record CheckoutRequest(string FirstName, string LastName, string AddressLine,
    string Country, string State, string ZipCode, PaymentMethod PaymentMethod);

public record CheckoutResponse(Guid CheckoutId);
