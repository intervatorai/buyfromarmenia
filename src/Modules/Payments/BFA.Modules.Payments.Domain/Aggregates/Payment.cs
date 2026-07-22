using BFA.BuildingBlocks.Domain;
using BFA.Modules.Payments.Domain.Enums;
using BFA.Modules.Payments.Domain.Events;

namespace BFA.Modules.Payments.Domain.Aggregates;

public sealed class Payment : AggregateRoot
{
    public Guid CustomerOrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentRecordStatus Status { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public string? ExternalReference { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CapturedAtUtc { get; private set; }

    private Payment()
    {
    }

    public Payment(Guid customerOrderId, decimal amount, string currency)
    {
        if (customerOrderId == Guid.Empty)
        {
            throw new DomainException("Customer order id is required.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Payment amount must be positive.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        Id = Guid.NewGuid();
        CustomerOrderId = customerOrderId;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        Status = PaymentRecordStatus.Pending;
        Provider = PaymentProvider.Stub;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Capture()
    {
        if (Status != PaymentRecordStatus.Pending)
        {
            throw new DomainException("Only pending payments can be captured.");
        }

        Status = PaymentRecordStatus.Captured;
        ExternalReference = $"STUB-{Id:N}";
        CapturedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new PaymentCapturedDomainEvent(Id, CustomerOrderId, Amount, Currency));
    }

    public void Fail()
    {
        if (Status != PaymentRecordStatus.Pending)
        {
            throw new DomainException("Only pending payments can fail.");
        }

        Status = PaymentRecordStatus.Failed;
    }
}
