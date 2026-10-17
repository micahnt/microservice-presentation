using Ecommerce.Shared.Messaging.Events;
using MassTransit;
using Microsoft.FeatureManagement;

namespace Ecommerce.Ordering.Application.Orders;

public interface IOrderCreator
{
    Task<Guid> CreateFromCheckout(BasketCheckoutEvent checkout);
}

public class OrderCreator(
    IApplicationDbContext db,
    IPublishEndpoint publisher,
    IFeatureManager features,
    IPaymentGateway payments,
    ILogger<OrderCreator> log) : IOrderCreator
{
    public async Task<Guid> CreateFromCheckout(BasketCheckoutEvent e)
    {
        var existing = await db.Orders.Where(o => o.CheckoutId == e.Id).Select(o => o.Id).FirstOrDefaultAsync();
        if (existing is not null)
        {
            log.LogInformation("Checkout {CheckoutId} already created order {OrderId}", e.Id, existing.Value);
            return existing.Value;
        }

        await EnsureCustomer(e);

        var orderId = OrderId.Of(Guid.NewGuid());
        var address = Address.Of(e.FirstName, e.LastName, e.EmailAddress, e.AddressLine, e.Country, e.State, e.ZipCode);
        var order = Order.Create(orderId, CustomerId.Of(e.CustomerId), OrderName.Of(OrderNumber(orderId)),
            address, address, (PaymentMethod)e.PaymentMethod, checkoutId: e.Id);

        foreach (var item in e.Items)
            order.Add(ProductId.Of(item.ProductId), item.ProductName, item.Quantity, item.Price);

        if (order.Payment.Method == PaymentMethod.FakeCard)
        {
            var result = await payments.Charge(orderId.Value, order.TotalPrice);
            if (result.Succeeded) order.MarkPaid(result.TransactionId!);
            else order.MarkPaymentFailed();
        }

        db.Orders.Add(order);

        // Catalog decrements stock on this event. A declined payment takes no stock.
        if (order.Status != OrderStatus.PaymentFailed && await features.IsEnabledAsync("OrderFullfilment"))
        {
            order.MarkStockTaken();                                // so a later cancel gives it back
            await publisher.Publish(order.ToOrderCreatedEvent()); // written to the outbox, sent after commit
        }

        await db.SaveChangesAsync(CancellationToken.None);       // customer + order + outbox row in one transaction
        return orderId.Value;
    }

    // Users live in the Identity service; Ordering keeps its own copy for the Orders.CustomerId FK.
    private async Task EnsureCustomer(BasketCheckoutEvent e)
    {
        var customerId = CustomerId.Of(e.CustomerId);
        if (!await db.Customers.AnyAsync(c => c.Id == customerId))
            db.Customers.Add(Customer.Create(customerId, e.UserName, e.EmailAddress));
    }

    private static string OrderNumber(OrderId id) =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{id.Value.ToString("N")[..6].ToUpperInvariant()}";
}
