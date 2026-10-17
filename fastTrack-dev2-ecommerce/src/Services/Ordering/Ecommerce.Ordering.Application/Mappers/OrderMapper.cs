using Ecommerce.Shared.Messaging.Events;

namespace Ecommerce.Ordering.Application.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order order) => new(
        Id: order.Id.Value,
        CustomerId: order.CustomerId.Value,
        OrderName: order.OrderName.Value,
        ShippingAddress: order.ShippingAddress.ToDto(),
        BillingAddress: order.BillingAddress.ToDto(),
        Payment: new PaymentDto(order.Payment.Method, order.Payment.TransactionId),
        Status: order.Status,
        TotalPrice: order.TotalPrice,
        CreatedAt: order.CreatedAt is { } created ? DateTime.SpecifyKind(created, DateTimeKind.Utc) : null, // stored as UTC
        CheckoutId: order.CheckoutId,
        OrderItems: order.OrderItems
            .Select(i => new OrderItemDto(i.ProductId.Value, i.ProductName, i.Quantity, i.Price))
            .ToList());

    public static AddressDto ToDto(this Address a) =>
        new(a.FirstName, a.LastName, a.EmailAddress!, a.AddressLine, a.Country, a.State, a.ZipCode);

    // The message contract is its own record, so the API DTO and the event can change independently.
    public static OrderCreatedEvent ToOrderCreatedEvent(this Order order) => new(
        order.Id.Value,
        order.CustomerId.Value,
        order.OrderItems.Select(i => new OrderCreatedItem(i.ProductId.Value, i.Quantity)).ToList());

    public static OrderCancelledEvent ToOrderCancelledEvent(this Order order) => new(
        order.Id.Value,
        order.OrderItems.Select(i => new OrderCancelledItem(i.ProductId.Value, i.Quantity)).ToList());
}
