namespace Ecommerce.Ordering.Api.Controllers;

// Orders are created only from BasketCheckoutEvent (see BasketCheckoutEventConsumer), not over HTTP.
[ApiController]
[Route("orders")]
public class OrdersController(IOrderReader reader, IOrderUpdater updater, IOrderDeleter deleter) : ControllerBase
{
    [HttpGet, Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetOrders([FromQuery] OrdersQuery query)
        => Ok(await reader.GetOrders(new PaginationRequest(query.PageIndex, query.PageSize), query.Name));

    [HttpGet("mine"), Authorize(Policy = Policies.Customer)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMine()
        => Ok(await reader.GetMine());

    [HttpGet("{id:guid}"), Authorize(Policy = Policies.Customer)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
        => Ok(await reader.GetById(id));

    [HttpGet("customer/{customerId:guid}"), Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomer(Guid customerId)
        => Ok(await reader.GetByCustomer(customerId));

    [HttpPut("{id:guid}/status"), Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request)
    {
        await updater.ChangeStatus(id, request.Status);
        return NoContent();
    }

    [HttpDelete("{id:guid}"), Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await deleter.Delete(id);
        return NoContent();
    }
}
