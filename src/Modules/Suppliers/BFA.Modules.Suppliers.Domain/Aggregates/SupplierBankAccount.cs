using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.ValueObjects;

namespace BFA.Modules.Suppliers.Domain.Aggregates;

public sealed class SupplierBankAccount : Entity
{
    public Guid SupplierId { get; private set; }
    public BankAccountDetails Details { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SupplierBankAccount()
    {
    }

    internal SupplierBankAccount(Guid supplierId, BankAccountDetails details, bool isPrimary = false)
    {
        Id = Guid.NewGuid();
        SupplierId = supplierId;
        Details = details;
        IsPrimary = isPrimary;
        CreatedAt = DateTime.UtcNow;
    }

    internal void Update(BankAccountDetails details)
    {
        Details = details;
    }

    internal void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
