namespace Ecommerce.Shared.Pagination;

public class PaginatedResult<TEntity>
    (int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
    where TEntity : class
{
    public int PageIndex { get; } = pageIndex;
    public int PageSize { get; } = pageSize;
    public long Count { get; } = count;
    public IEnumerable<TEntity> Data { get; } = data;

    public PaginatedResult<TOut> Map<TOut>(Func<TEntity, TOut> map) where TOut : class =>
        new(PageIndex, PageSize, Count, Data.Select(map).ToList());
}
