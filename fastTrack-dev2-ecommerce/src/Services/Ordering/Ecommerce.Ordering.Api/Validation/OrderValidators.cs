namespace Ecommerce.Ordering.Api.Validation;

public class OrdersQueryValidator : AbstractValidator<OrdersQuery>
{
    public OrdersQueryValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum()
            .Must(s => s is OrderStatus.Shipped or OrderStatus.Completed or OrderStatus.Cancelled)
            .WithMessage("Status must be Shipped, Completed or Cancelled.");
    }
}
