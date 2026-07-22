using BFA.BuildingBlocks.Domain;
using BFA.Modules.Settlements.Domain.Enums;

namespace BFA.Modules.Settlements.Domain.Aggregates;

public sealed class SupplierSettlement : AggregateRoot
{
    public Guid SupplierId { get; private set; }
    public Guid SupplierOrderId { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal PlatformFee { get; private set; }
    public decimal NetAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public SettlementStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? EligibleAtUtc { get; private set; }

    private SupplierSettlement()
    {
    }

    public static SupplierSettlement CreateFromSupplierOrder(
        Guid supplierId,
        Guid supplierOrderId,
        decimal grossAmount,
        string currency,
        decimal platformFeeRate = 0.15m)
    {
        if (supplierId == Guid.Empty || supplierOrderId == Guid.Empty)
        {
            throw new DomainException("Supplier and supplier order are required.");
        }

        if (grossAmount <= 0)
        {
            throw new DomainException("Gross amount must be positive.");
        }

        var platformFee = Math.Round(grossAmount * platformFeeRate, 2);

        return new SupplierSettlement
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            SupplierOrderId = supplierOrderId,
            GrossAmount = grossAmount,
            PlatformFee = platformFee,
            NetAmount = grossAmount - platformFee,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = SettlementStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            EligibleAtUtc = DateTime.UtcNow.AddDays(14)
        };
    }

    public void MarkEligible()
    {
        if (Status != SettlementStatus.Pending)
        {
            throw new DomainException("Only pending settlements can become eligible.");
        }

        Status = SettlementStatus.Eligible;
    }

    public void MarkPaid()
    {
        if (Status != SettlementStatus.Eligible)
        {
            throw new DomainException("Only eligible settlements can be paid.");
        }

        Status = SettlementStatus.Paid;
    }
}
