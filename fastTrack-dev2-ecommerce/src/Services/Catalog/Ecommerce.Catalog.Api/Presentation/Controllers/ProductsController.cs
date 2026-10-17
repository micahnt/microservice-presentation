namespace Ecommerce.Catalog.Api.Presentation.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(
    IProductReader reader, IProductCreator creator, IProductUpdater updater, IProductDeleter deleter)
    : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> Search([FromQuery] ProductQuery query)
        => Ok((await reader.Search(query)).Map(p => p.ToDto()));

    [HttpGet("{id:guid}"), AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
        => Ok((await reader.GetById(id)).ToDto());

    [HttpGet("category/{category}"), AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(string category)
        => Ok((await reader.GetByCategory(category)).Select(p => p.ToDto()));

    [HttpGet("categories"), AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        => Ok(await reader.GetCategories());

    [HttpPost, Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
    {
        var created = await creator.Create(request.ToEntity());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    [HttpPut("{id:guid}"), Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request)
    {
        await updater.Update(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}"), Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await deleter.Delete(id);
        return NoContent();
    }
}
