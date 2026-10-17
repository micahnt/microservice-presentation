using Ecommerce.Ordering.Application.Payments;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Ordering.Infrastructure.Payments;

// Approves everything up to a limit. To demo a declined payment, order something expensive.
public class FakePaymentGateway(ILogger<FakePaymentGateway> log) : IPaymentGateway
{
    private const decimal ApprovalLimit = 5_000m;

    public Task<PaymentResult> Charge(Guid orderId, decimal amount)
    {
        var approved = amount <= ApprovalLimit;
        log.LogInformation("Fake charge {OrderId} {Amount}: {Result}", orderId, amount, approved ? "approved" : "declined");

        return Task.FromResult(approved
            ? new PaymentResult(true, $"FAKE-{Guid.NewGuid():N}"[..13], null)
            : new PaymentResult(false, null, "Declined by fake gateway"));
    }
}
