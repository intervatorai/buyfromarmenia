namespace BFA.Modules.Ordering.Domain.Enums;

/// <summary>
/// Customer-facing order progress (matches Public UI tracking timeline).
/// </summary>
public enum CustomerTrackingStage
{
    OrderPlaced = 0,
    Confirmed = 1,
    BeingPrepared = 2,
    AtWarehouse = 3,
    Shipped = 4,
    InTransit = 5,
    OutForDelivery = 6,
    Delivered = 7
}
