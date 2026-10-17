namespace Ecommerce.Shared.Messaging.Events;

// Id and OccurredOnUtc are set once, so a redelivered message keeps the same Id (de-duplication).
public record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
