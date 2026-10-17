namespace Ecommerce.Ordering.Domain.ValueObjects;

// Payment method + the gateway's transaction id. No card data is ever stored.
public record Payment
{
    public PaymentMethod Method { get; } = default!;
    public string? TransactionId { get; }

    protected Payment()
    {
    }

    private Payment(PaymentMethod method, string? transactionId)
    {
        Method = method;
        TransactionId = transactionId;
    }

    public static Payment Of(PaymentMethod method, string? transactionId = null)
    {
        if (!Enum.IsDefined(method))
            throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown payment method.");

        return new Payment(method, transactionId);
    }
}
