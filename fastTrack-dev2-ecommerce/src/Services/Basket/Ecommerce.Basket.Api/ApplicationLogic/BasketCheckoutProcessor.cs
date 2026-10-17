using Ecommerce.Shared.Messaging.Events;
using MassTransit;

namespace Ecommerce.Basket.Api.ApplicationLogic;

public interface IBasketCheckoutProcessor
{
    Task<Guid> Checkout(CheckoutRequest request);
}

// Basket has no relational DB, so no outbox: it publishes directly. If RabbitMQ is down the call
// fails and the basket stays, so the user can retry. Ordering ignores duplicates by event Id.
public class BasketCheckoutProcessor(
    IBasketRepository repository, IPublishEndpoint publisher, ICurrentUser currentUser) : IBasketCheckoutProcessor
{
    public async Task<Guid> Checkout(CheckoutRequest request)
    {
        var cart = await repository.Get(currentUser.UserId);
        if (cart.Items.Count == 0)
            throw new BadRequestException("Basket is empty.");

        var checkout = new BasketCheckoutEvent
        {
            CustomerId = currentUser.UserId,
            UserName = currentUser.Name,
            EmailAddress = currentUser.Email,
            TotalPrice = cart.TotalPrice,
            Items = cart.Items
                .Select(i => new BasketCheckoutItem(i.ProductId, i.ProductName, i.Price, i.Quantity))
                .ToList(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            AddressLine = request.AddressLine,
            Country = request.Country,
            State = request.State,
            ZipCode = request.ZipCode,
            PaymentMethod = (int)request.PaymentMethod,
        };

        await publisher.Publish(checkout);
        await repository.Delete(currentUser.UserId);
        return checkout.Id;
    }
}
