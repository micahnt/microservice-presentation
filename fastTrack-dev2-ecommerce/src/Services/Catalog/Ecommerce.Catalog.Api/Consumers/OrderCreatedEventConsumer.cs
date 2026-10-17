using Ecommerce.Shared.Messaging.Events;
using MassTransit;

namespace Ecommerce.Catalog.Api.Consumers;

public class OrderCreatedEventConsumer(IProductUpdater updater, ILogger<OrderCreatedEventConsumer> log)
    : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        log.LogInformation("Decrementing stock for order {OrderId} (event {EventId})",
            context.Message.OrderId, context.Message.Id);

        await updater.DecrementStock(context.Message.Items.Select(i => (i.ProductId, i.Quantity)).ToList());
    }
}
