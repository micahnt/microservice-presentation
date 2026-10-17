using Ecommerce.Shared.Messaging.Events;
using MassTransit;

namespace Ecommerce.Catalog.Api.Consumers;

public class OrderCancelledEventConsumer(IProductUpdater updater, ILogger<OrderCancelledEventConsumer> log)
    : IConsumer<OrderCancelledEvent>
{
    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        log.LogInformation("Returning stock for cancelled order {OrderId} (event {EventId})",
            context.Message.OrderId, context.Message.Id);

        await updater.IncrementStock(context.Message.Items.Select(i => (i.ProductId, i.Quantity)).ToList());
    }
}
