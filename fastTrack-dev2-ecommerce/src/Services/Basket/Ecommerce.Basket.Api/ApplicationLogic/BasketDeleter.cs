namespace Ecommerce.Basket.Api.ApplicationLogic;

public interface IBasketDeleter
{
    Task Delete();
}

public class BasketDeleter(IBasketRepository repository, ICurrentUser currentUser) : IBasketDeleter
{
    public Task Delete() => repository.Delete(currentUser.UserId);
}
