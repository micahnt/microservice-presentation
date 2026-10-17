using MassTransit;

namespace Ecommerce.Ordering.Application.Orders;

public interface IOrderUpdater
{
    Task ChangeStatus(Guid id, OrderStatus status);
}

// Admin moves an order along: Pending/Paid -> Shipped -> Completed, or Cancelled.
// Paid / PaymentFailed are set only by the payment gateway.
// A cancelled order that took stock publishes OrderCancelledEvent, so Catalog gives the stock back.
// Refunds for cancelled Paid orders are not implemented (see template 7.1.1).
public class OrderUpdater(IApplicationDbContext db, IPublishEndpoint publisher) : IOrderUpdater
{
    public async Task ChangeStatus(Guid id, OrderStatus status)
    {
        // Items are loaded so the computed TotalPrice column keeps its value when the order is saved.
        var orderId = OrderId.Of(id);
        var order = await db.Orders.Include(o => o.OrderItems).SingleOrDefaultAsync(o => o.Id == orderId)
            ?? throw new OrderNotFoundException(id);

        try
        {
            switch (status)
            {
                case OrderStatus.Shipped: order.Ship(); break;
                case OrderStatus.Completed: order.Complete(); break;
                case OrderStatus.Cancelled:
                    if (order.Cancel())
                        await publisher.Publish(order.ToOrderCancelledEvent()); // outbox: sent after the commit below
                    break;
                default: throw new BadRequestException($"Status {status} can't be set manually.");
            }
        }
        catch (DomainException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await db.SaveChangesAsync(CancellationToken.None);
    }
}
