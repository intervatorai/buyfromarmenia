using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Inventory.Domain.Events;

public sealed record LowStockDetectedDomainEvent(
    Guid StockItemId,
    Guid ProductVariantId,
    int Available,
    int LowStockThreshold) : DomainEvent;

public sealed record StockReservedDomainEvent(
    Guid StockItemId,
    Guid ReservationId,
    int Quantity) : DomainEvent;

public sealed record StockReservationReleasedDomainEvent(
    Guid StockItemId,
    Guid ReservationId,
    int Quantity) : DomainEvent;
