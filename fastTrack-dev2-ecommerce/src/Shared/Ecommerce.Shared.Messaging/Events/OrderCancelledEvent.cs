namespace Ecommerce.Shared.Messaging.Events;

// Published by Ordering (through the outbox) when an order that took stock is cancelled.
// Consumed by Catalog to give the stock back.
public record OrderCancelledEvent(Guid OrderId, List<OrderCancelledItem> Items) : IntegrationEvent;

public record OrderCancelledItem(Guid ProductId, int Quantity);
