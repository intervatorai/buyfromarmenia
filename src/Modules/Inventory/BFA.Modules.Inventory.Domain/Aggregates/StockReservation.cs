using BFA.BuildingBlocks.Domain;
using BFA.Modules.Inventory.Domain.Enums;

namespace BFA.Modules.Inventory.Domain.Aggregates;

public sealed class StockReservation : Entity
{
    public Guid StockItemId { get; private set; }
    public Guid ReferenceId { get; private set; }
    public int Quantity { get; private set; }
    public StockReservationStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private StockReservation()
    {
    }

    internal StockReservation(
        Guid stockItemId,
        Guid referenceId,
        int quantity,
        DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        StockItemId = stockItemId;
        ReferenceId = referenceId;
        Quantity = quantity;
        Status = StockReservationStatus.Active;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    internal void Confirm()
    {
        EnsureActive();
        Status = StockReservationStatus.Confirmed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    internal void Release(bool expired = false)
    {
        EnsureActive();
        Status = expired
            ? StockReservationStatus.Expired
            : StockReservationStatus.Released;
        CompletedAtUtc = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != StockReservationStatus.Active)
        {
            throw new DomainException("Only active stock reservations can be changed.");
        }
    }
}
