namespace Ecommerce.Ordering.Domain.Events;

public record OrderCreatedEvent(Order order) : IDomainEvent;
