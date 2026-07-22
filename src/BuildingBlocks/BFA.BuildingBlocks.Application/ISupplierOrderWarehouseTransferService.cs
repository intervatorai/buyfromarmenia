namespace BFA.BuildingBlocks.Application;

public interface ISupplierOrderWarehouseTransferService
{
    /// <summary>
    /// Advances a supplier order through Confirm → Preparing → ReadyForPickup when needed.
    /// </summary>
    Task AdvanceToReadyForPickupAsync(
        Guid supplierOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfers a ReadyForPickup supplier order to warehouse (inbound + settlement).
    /// Idempotent when inbound already exists.
    /// </summary>
    Task<bool> TransferReadyOrderToWarehouseAsync(
        Guid supplierOrderId,
        CancellationToken cancellationToken = default);
}
