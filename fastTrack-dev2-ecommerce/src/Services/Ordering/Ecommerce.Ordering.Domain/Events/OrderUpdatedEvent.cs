namespace Ecommerce.Ordering.Domain.Events;

public record OrderUpdatedEvent(Order order) : IDomainEvent;
