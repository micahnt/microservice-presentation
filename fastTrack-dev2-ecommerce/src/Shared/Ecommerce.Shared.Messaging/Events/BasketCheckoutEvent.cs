namespace Ecommerce.Shared.Messaging.Events;

public record BasketCheckoutEvent : IntegrationEvent
{
    public Guid CustomerId { get; init; }
    public string UserName { get; init; } = default!;
    public decimal TotalPrice { get; init; }
    public List<BasketCheckoutItem> Items { get; init; } = [];

    // Shipping and billing address
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string EmailAddress { get; init; } = default!;
    public string AddressLine { get; init; } = default!;
    public string Country { get; init; } = default!;
    public string State { get; init; } = default!;
    public string ZipCode { get; init; } = default!;

    // Payment: method only, never card data. 1 = CashOnDelivery, 2 = FakeCard
    public int PaymentMethod { get; init; }
}

public record BasketCheckoutItem(Guid ProductId, string ProductName, decimal Price, int Quantity);
