namespace Ecommerce.Ordering.Domain.Models;

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public CustomerId CustomerId { get; private set; } = default!;
    public OrderName OrderName { get; private set; } = default!;
    public Address ShippingAddress { get; private set; } = default!;
    public Address BillingAddress { get; private set; } = default!;
    public Payment Payment { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    // Id of the BasketCheckoutEvent that created the order (unique): a redelivered event can't create a second order.
    public Guid? CheckoutId { get; private set; }

    // True once OrderCreatedEvent was published, i.e. Catalog took stock for this order.
    // A PaymentFailed order, or one created while stock updates were off, never took stock.
    public bool StockTaken { get; private set; }

    public decimal TotalPrice
    {
        get => OrderItems.Sum(x => x.Price * x.Quantity);
        private set { }
    }

    public static Order Create(OrderId id, CustomerId customerId, OrderName orderName,
        Address shippingAddress, Address billingAddress, PaymentMethod paymentMethod, Guid? checkoutId = null)
    {
        return new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = Payment.Of(paymentMethod),
            Status = OrderStatus.Pending,
            CheckoutId = checkoutId,
        };
    }

    public void Add(ProductId productId, string productName, int quantity, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(price); // a full discount makes a line free

        _orderItems.Add(new OrderItem(Id, productId, productName, quantity, price));
    }

    public void MarkPaid(string transactionId)
    {
        EnsureStatus(OrderStatus.Pending);
        Payment = Payment.Of(Payment.Method, transactionId);
        Status = OrderStatus.Paid;
    }

    public void MarkPaymentFailed()
    {
        EnsureStatus(OrderStatus.Pending);
        Status = OrderStatus.PaymentFailed;
    }

    // Paid orders, or cash-on-delivery orders that are still pending.
    public void Ship()
    {
        var canShip = Status == OrderStatus.Paid
            || (Status == OrderStatus.Pending && Payment.Method == PaymentMethod.CashOnDelivery);
        if (!canShip)
            throw new DomainException($"A {Status} order paid by {Payment.Method} can't be shipped.");
        Status = OrderStatus.Shipped;
    }

    public void Complete()
    {
        EnsureStatus(OrderStatus.Shipped);
        Status = OrderStatus.Completed;
    }

    public void MarkStockTaken()
    {
        if (Status == OrderStatus.PaymentFailed)
            throw new DomainException("A PaymentFailed order doesn't take stock.");
        StockTaken = true;
    }

    // Returns true when the order had taken stock, so the caller must give it back.
    public bool Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Completed or OrderStatus.Cancelled)
            throw new DomainException($"A {Status} order can't be cancelled.");
        Status = OrderStatus.Cancelled;

        var releaseStock = StockTaken;
        StockTaken = false;
        return releaseStock;
    }

    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
            throw new DomainException($"Order is {Status}, expected {expected}.");
    }
}
