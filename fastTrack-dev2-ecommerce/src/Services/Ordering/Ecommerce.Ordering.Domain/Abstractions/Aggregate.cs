namespace Ecommerce.Ordering.Domain.Abstractions;

// Marks the root of a consistency boundary. Domain events were removed together with MediatR;
// integration events are published by the use cases through the MassTransit outbox instead.
public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
}

public interface IAggregate<T> : IAggregate, IEntity<T>
{
}

public interface IAggregate : IEntity
{
}
