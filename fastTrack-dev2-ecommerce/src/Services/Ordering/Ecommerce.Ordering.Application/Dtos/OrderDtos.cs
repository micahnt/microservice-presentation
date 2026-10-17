namespace Ecommerce.Ordering.Application.Dtos;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string OrderName,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    PaymentDto Payment,
    OrderStatus Status,
    decimal TotalPrice,
    DateTime? CreatedAt,
    Guid? CheckoutId,
    List<OrderItemDto> OrderItems);

public record AddressDto(string FirstName, string LastName, string EmailAddress, string AddressLine,
    string Country, string State, string ZipCode);

public record PaymentDto(PaymentMethod Method, string? TransactionId);

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);
