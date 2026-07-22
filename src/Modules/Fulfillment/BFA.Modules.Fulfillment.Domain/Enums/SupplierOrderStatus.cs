namespace BFA.Modules.Fulfillment.Domain.Enums;

public enum SupplierOrderStatus
{
    New = 0,
    Confirmed = 1,
    Preparing = 2,
    ReadyForPickup = 3,
    TransferredToWarehouse = 4,
    Cancelled = 5
}
