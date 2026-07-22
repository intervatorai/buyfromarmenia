using BFA.BuildingBlocks.Domain;
using BFA.Modules.Settlements.Domain.Enums;

namespace BFA.Modules.Settlements.Domain.Aggregates;

public sealed class Payout : AggregateRoot
{
    public Guid SupplierId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PayoutStatus Status { get; private set; }
    public DateTime ScheduledForUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private Payout()
    {
    }

    public Payout(Guid supplierId, decimal amount, string currency, DateTime scheduledForUtc)
    {
        if (supplierId == Guid.Empty)
        {
            throw new DomainException("Supplier id is required.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Payout amount must be positive.");
        }

        Id = Guid.NewGuid();
        SupplierId = supplierId;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        Status = PayoutStatus.Scheduled;
        ScheduledForUtc = scheduledForUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        if (Status != PayoutStatus.Scheduled)
        {
            throw new DomainException("Only scheduled payouts can be completed.");
        }

        Status = PayoutStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }
}
