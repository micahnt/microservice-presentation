namespace Ecommerce.Basket.Api.ApplicationLogic;

public interface IBasketReader
{
    Task<ShoppingCart> Get();
}

// Always returns a basket: an empty one when the user has none yet (no 404).
public class BasketReader(IBasketRepository repository, ICurrentUser currentUser) : IBasketReader
{
    public Task<ShoppingCart> Get() => repository.Get(currentUser.UserId);
}
