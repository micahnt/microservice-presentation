namespace Ecommerce.Ordering.Application.Orders;

public interface IOrderReader
{
    Task<PaginatedResult<OrderDto>> GetOrders(PaginationRequest page, string? name);
    Task<OrderDto> GetById(Guid id);
    Task<List<OrderDto>> GetMine();
    Task<List<OrderDto>> GetByCustomer(Guid customerId);
}

public class OrderReader(IApplicationDbContext db, ICurrentUser currentUser) : IOrderReader
{
    private IQueryable<Order> Orders => db.Orders.Include(o => o.OrderItems).AsNoTracking();

    public async Task<PaginatedResult<OrderDto>> GetOrders(PaginationRequest page, string? name)
    {
        var query = Orders;
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(o => o.OrderName.Value.Contains(name));

        var total = await query.LongCountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip(page.PageSize * page.PageIndex)
            .Take(page.PageSize)
            .ToListAsync();

        return new PaginatedResult<OrderDto>(page.PageIndex, page.PageSize, total, orders.Select(o => o.ToDto()).ToList());
    }

    // Customers can only see their own orders; any other id looks like a missing order.
    public async Task<OrderDto> GetById(Guid id)
    {
        var orderId = OrderId.Of(id);
        var order = await Orders.SingleOrDefaultAsync(o => o.Id == orderId);
        if (order is null || (!currentUser.IsAdmin && order.CustomerId.Value != currentUser.UserId))
            throw new OrderNotFoundException(id);
        return order.ToDto();
    }

    public Task<List<OrderDto>> GetMine() => GetByCustomer(currentUser.UserId);

    public async Task<List<OrderDto>> GetByCustomer(Guid customerId)
    {
        var id = CustomerId.Of(customerId);
        var orders = await Orders
            .Where(o => o.CustomerId == id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(o => o.ToDto()).ToList();
    }
}
