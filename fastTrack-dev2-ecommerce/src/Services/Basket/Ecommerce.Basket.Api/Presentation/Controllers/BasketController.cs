namespace Ecommerce.Basket.Api.Presentation.Controllers;

// Every route works on the current user's basket (user id from the token, never from the URL).
[ApiController]
[Route("basket")]
[Authorize(Policy = Policies.Customer)]
public class BasketController(
    IBasketReader reader, IBasketStorer storer, IBasketDeleter deleter, IBasketCheckoutProcessor checkout)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BasketDto>> Get() => Ok((await reader.Get()).ToDto());

    [HttpPost]
    public async Task<ActionResult<BasketDto>> Store(StoreBasketRequest request)
        => Ok((await storer.Store(request.Items)).ToDto());

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        await deleter.Delete();
        return NoContent();
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout(CheckoutRequest request)
        => Accepted(new CheckoutResponse(await checkout.Checkout(request)));
}
