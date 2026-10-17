namespace Ecommerce.Ordering.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    PaymentFailed = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6,
}
