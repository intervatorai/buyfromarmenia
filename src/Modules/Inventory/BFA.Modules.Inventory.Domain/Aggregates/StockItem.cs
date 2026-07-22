using BFA.BuildingBlocks.Domain;
using BFA.Modules.Inventory.Domain.Enums;
using BFA.Modules.Inventory.Domain.Events;

namespace BFA.Modules.Inventory.Domain.Aggregates;

public sealed class StockItem : AggregateRoot
{
    private readonly List<StockReservation> _reservations = [];

    public Guid SupplierId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int OnHand { get; private set; }
    public int Reserved { get; private set; }
    public int Available => OnHand - Reserved;
    public int LowStockThreshold { get; private set; }
    public uint RowVersion { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    private StockItem()
    {
    }

    public StockItem(
        Guid supplierId,
        Guid productId,
        Guid productVariantId,
        int onHand = 0,
        int lowStockThreshold = 5)
    {
        if (supplierId == Guid.Empty || productId == Guid.Empty || productVariantId == Guid.Empty)
        {
            throw new DomainException("Supplier, product and product variant are required.");
        }

        ValidateQuantity(onHand, nameof(onHand));
        ValidateQuantity(lowStockThreshold, nameof(lowStockThreshold));

        Id = Guid.NewGuid();
        SupplierId = supplierId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        OnHand = onHand;
        LowStockThreshold = lowStockThreshold;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        RaiseLowStockIfNeeded();
    }

    public void SetOnHand(int quantity, int lowStockThreshold)
    {
        ValidateQuantity(quantity, nameof(quantity));
        ValidateQuantity(lowStockThreshold, nameof(lowStockThreshold));

        if (quantity < Reserved)
        {
            throw new DomainException("On-hand stock cannot be lower than reserved stock.");
        }

        OnHand = quantity;
        LowStockThreshold = lowStockThreshold;
        UpdatedAtUtc = DateTime.UtcNow;
        RaiseLowStockIfNeeded();
    }

    public StockReservation Reserve(Guid referenceId, int quantity, DateTime expiresAtUtc)
    {
        if (referenceId == Guid.Empty)
        {
            throw new DomainException("Reservation reference is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Reservation quantity must be positive.");
        }

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            throw new DomainException("Reservation expiry must be in the future.");
        }

        if (quantity > Available)
        {
            throw new DomainException("Insufficient available stock.");
        }

        var reservation = new StockReservation(Id, referenceId, quantity, expiresAtUtc);
        _reservations.Add(reservation);
        Reserved += quantity;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new StockReservedDomainEvent(Id, reservation.Id, quantity));
        RaiseLowStockIfNeeded();
        return reservation;
    }

    public void ReleaseReservation(Guid reservationId)
    {
        var reservation = GetActiveReservation(reservationId);
        reservation.Release();
        Reserved -= reservation.Quantity;
        UpdatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new StockReservationReleasedDomainEvent(
            Id,
            reservation.Id,
            reservation.Quantity));
    }

    public void ConfirmReservation(Guid reservationId)
    {
        var reservation = GetActiveReservation(reservationId);
        reservation.Confirm();
        Reserved -= reservation.Quantity;
        OnHand -= reservation.Quantity;
        UpdatedAtUtc = DateTime.UtcNow;
        RaiseLowStockIfNeeded();
    }

    public void ExpireReservations(DateTime utcNow)
    {
        foreach (var reservation in _reservations.Where(reservation =>
                     reservation.Status == StockReservationStatus.Active
                     && reservation.ExpiresAtUtc <= utcNow))
        {
            reservation.Release(expired: true);
            Reserved -= reservation.Quantity;
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    private StockReservation GetActiveReservation(Guid reservationId)
    {
        return _reservations.FirstOrDefault(reservation =>
                   reservation.Id == reservationId
                   && reservation.Status == StockReservationStatus.Active)
               ?? throw new DomainException("Active stock reservation was not found.");
    }

    private void RaiseLowStockIfNeeded()
    {
        if (Available <= LowStockThreshold)
        {
            RaiseDomainEvent(new LowStockDetectedDomainEvent(
                Id,
                ProductVariantId,
                Available,
                LowStockThreshold));
        }
    }

    private static void ValidateQuantity(int quantity, string name)
    {
        if (quantity < 0)
        {
            throw new DomainException($"{name} cannot be negative.");
        }
    }
}
