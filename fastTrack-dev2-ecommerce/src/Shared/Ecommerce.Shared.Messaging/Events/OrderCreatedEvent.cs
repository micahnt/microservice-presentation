namespace Ecommerce.Shared.Messaging.Events;

// Published by Ordering (through the outbox), consumed by Catalog to decrement stock.
public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, List<OrderCreatedItem> Items) : IntegrationEvent;

public record OrderCreatedItem(Guid ProductId, int Quantity);
