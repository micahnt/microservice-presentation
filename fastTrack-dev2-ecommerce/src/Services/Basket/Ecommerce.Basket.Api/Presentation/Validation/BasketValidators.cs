namespace Ecommerce.Basket.Api.Presentation.Validation;

public class StoreBasketRequestValidator : AbstractValidator<StoreBasketRequest>
{
    public StoreBasketRequestValidator()
    {
        RuleFor(x => x.Items).NotNull();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 99);
        });
    }
}

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(50);
        RuleFor(x => x.State).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}
