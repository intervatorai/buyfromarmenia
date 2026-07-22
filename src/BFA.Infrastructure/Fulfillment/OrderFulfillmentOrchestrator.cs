using BFA.BuildingBlocks.Application;
using BFA.Modules.Fulfillment.Domain.Enums;
using BFA.Modules.Fulfillment.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BFA.Infrastructure.Fulfillment;

public sealed class OrderFulfillmentOrchestrator : IOrderFulfillmentOrchestrator
{
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly ISupplierOrderWarehouseTransferService _warehouseTransferService;
    private readonly ILogger<OrderFulfillmentOrchestrator> _logger;

    public OrderFulfillmentOrchestrator(
        ISupplierOrderRepository supplierOrderRepository,
        ISupplierOrderWarehouseTransferService warehouseTransferService,
        ILogger<OrderFulfillmentOrchestrator> logger)
    {
        _supplierOrderRepository = supplierOrderRepository;
        _warehouseTransferService = warehouseTransferService;
        _logger = logger;
    }

    public async Task StartForOrderAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        var supplierOrders = await _supplierOrderRepository.GetByCustomerOrderIdAsync(
            customerOrderId,
            cancellationToken);

        _logger.LogInformation(
            "OrderFulfillmentProcess started for customer order {CustomerOrderId} with {SupplierOrderCount} supplier order(s).",
            customerOrderId,
            supplierOrders.Count);

        var transferred = 0;

        foreach (var summary in supplierOrders)
        {
            if (summary.Status == SupplierOrderStatus.TransferredToWarehouse)
            {
                continue;
            }

            await _warehouseTransferService.AdvanceToReadyForPickupAsync(
                summary.Id,
                cancellationToken);

            var didTransfer = await _warehouseTransferService.TransferReadyOrderToWarehouseAsync(
                summary.Id,
                cancellationToken);

            if (didTransfer)
            {
                transferred++;
            }
        }

        _logger.LogInformation(
            "OrderFulfillmentProcess finished for customer order {CustomerOrderId}: {TransferredCount} inbound shipment(s) created.",
            customerOrderId,
            transferred);
    }
}
