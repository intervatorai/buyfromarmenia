namespace BFA.Modules.Shipping.Domain.Enums;

public enum ShipmentStatus
{
    Created = 0,
    PickedUp = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4
}
