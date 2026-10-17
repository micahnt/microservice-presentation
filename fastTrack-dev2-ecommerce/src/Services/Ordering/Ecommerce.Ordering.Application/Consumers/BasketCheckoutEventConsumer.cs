using Ecommerce.Ordering.Application.Orders;
using Ecommerce.Shared.Messaging.Events;
using MassTransit;

namespace Ecommerce.Ordering.Application.Consumers;

public class BasketCheckoutEventConsumer(IOrderCreator creator, ILogger<BasketCheckoutEventConsumer> log)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        log.LogInformation("Creating order for checkout {CheckoutId}", context.Message.Id);
        var orderId = await creator.CreateFromCheckout(context.Message);
        log.LogInformation("Checkout {CheckoutId} -> order {OrderId}", context.Message.Id, orderId);
    }
}
