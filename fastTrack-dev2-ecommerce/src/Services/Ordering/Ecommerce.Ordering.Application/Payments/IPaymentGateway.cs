namespace Ecommerce.Ordering.Application.Payments;

// Implemented by FakePaymentGateway (Infrastructure). Swap for Stripe Checkout when real payments are needed.
public interface IPaymentGateway
{
    Task<PaymentResult> Charge(Guid orderId, decimal amount);
}

public record PaymentResult(bool Succeeded, string? TransactionId, string? Error);
