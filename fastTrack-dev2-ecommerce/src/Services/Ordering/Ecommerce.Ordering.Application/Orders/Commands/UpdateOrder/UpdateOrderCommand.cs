using Ecommerce.Shared.CQRS;
using FluentValidation;
using Ecommerce.Ordering.Application.Dtos;

namespace Ecommerce.Ordering.Application.Orders.Commands.UpdateOrder;
public record UpdateOrderCommand(OrderDto Order)
    : ICommand<UpdateOrderResult>;

public record UpdateOrderResult(bool IsSuccess);

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Order.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Order.CustomerId).NotNull().WithMessage("CustomerId is required");
    }
}

