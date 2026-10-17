using System.Text.Json;
using StackExchange.Redis;

namespace Ecommerce.Basket.Api.Data;

public interface IBasketRepository
{
    Task<ShoppingCart> Get(Guid userId);
    Task Save(ShoppingCart cart);
    Task Delete(Guid userId);
}

// A basket is temporary data: one Redis key per user, expiry refreshed on every write.
public class BasketRepository(IConnectionMultiplexer redis) : IBasketRepository
{
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(30);
    private readonly IDatabase _db = redis.GetDatabase();

    private static string Key(Guid userId) => $"basket:{userId}";

    public async Task<ShoppingCart> Get(Guid userId)
    {
        var json = await _db.StringGetAsync(Key(userId));
        return json.IsNullOrEmpty ? new ShoppingCart(userId) : JsonSerializer.Deserialize<ShoppingCart>(json!)!;
    }

    public Task Save(ShoppingCart cart) =>
        _db.StringSetAsync(Key(cart.UserId), JsonSerializer.Serialize(cart), Ttl);

    public Task Delete(Guid userId) => _db.KeyDeleteAsync(Key(userId));
}
