namespace Ecommerce.Ordering.Application.Orders;

public interface IOrderDeleter
{
    Task Delete(Guid id);
}

public class OrderDeleter(IApplicationDbContext db) : IOrderDeleter
{
    public async Task Delete(Guid id)
    {
        var order = await db.Orders.FindAsync([OrderId.Of(id)]) ?? throw new OrderNotFoundException(id);
        db.Orders.Remove(order);
        await db.SaveChangesAsync(CancellationToken.None);
    }
}
