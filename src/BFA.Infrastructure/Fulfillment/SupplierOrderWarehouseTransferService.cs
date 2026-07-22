using BFA.BuildingBlocks.Application;
using BFA.Modules.Fulfillment.Domain.Enums;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Settlements.Domain.Aggregates;
using BFA.Modules.Settlements.Domain.Repositories;
using BFA.Modules.Warehouse.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BFA.Infrastructure.Fulfillment;

public sealed class SupplierOrderWarehouseTransferService : ISupplierOrderWarehouseTransferService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly IInboundShipmentRepository _inboundShipmentRepository;
    private readonly ISupplierSettlementRepository _settlementRepository;
    private readonly ILogger<SupplierOrderWarehouseTransferService> _logger;

    public SupplierOrderWarehouseTransferService(
        IUnitOfWork unitOfWork,
        ISupplierOrderRepository supplierOrderRepository,
        IInboundShipmentRepository inboundShipmentRepository,
        ISupplierSettlementRepository settlementRepository,
        ILogger<SupplierOrderWarehouseTransferService> logger)
    {
        _unitOfWork = unitOfWork;
        _supplierOrderRepository = supplierOrderRepository;
        _inboundShipmentRepository = inboundShipmentRepository;
        _settlementRepository = settlementRepository;
        _logger = logger;
    }

    public async Task AdvanceToReadyForPickupAsync(
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _supplierOrderRepository.GetByIdForUpdateAsync(
            supplierOrderId,
            cancellationToken);

        if (order is null)
        {
            return;
        }

        if (order.Status is SupplierOrderStatus.ReadyForPickup
            or SupplierOrderStatus.TransferredToWarehouse)
        {
            return;
        }

        switch (order.Status)
        {
            case SupplierOrderStatus.New:
                order.Confirm();
                order.MarkPreparing();
                order.MarkReadyForPickup();
                break;
            case SupplierOrderStatus.Confirmed:
                order.MarkPreparing();
                order.MarkReadyForPickup();
                break;
            case SupplierOrderStatus.Preparing:
                order.MarkReadyForPickup();
                break;
            default:
                return;
        }

        await _supplierOrderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation(
            "Supplier order {SupplierOrderId} advanced to ReadyForPickup.",
            supplierOrderId);
    }

    public async Task<bool> TransferReadyOrderToWarehouseAsync(
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        var existingShipment = await _inboundShipmentRepository.GetBySupplierOrderIdAsync(
            supplierOrderId,
            cancellationToken);
        if (existingShipment is not null)
        {
            return false;
        }

        var order = await _supplierOrderRepository.GetByIdForUpdateAsync(
            supplierOrderId,
            cancellationToken);

        if (order is null || order.Status != SupplierOrderStatus.ReadyForPickup)
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            order.MarkTransferredToWarehouse();
            var shipment = InboundShipment.CreateFromSupplierOrder(order);

            await _supplierOrderRepository.UpdateAsync(order, cancellationToken);
            await _inboundShipmentRepository.AddAsync(shipment, cancellationToken);

            var settlement = SupplierSettlement.CreateFromSupplierOrder(
                order.SupplierId,
                order.Id,
                order.Subtotal,
                order.Currency);
            await _settlementRepository.AddAsync(settlement, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Supplier order {SupplierOrderId} transferred to warehouse as inbound {InboundShipmentId}.",
                supplierOrderId,
                shipment.Id);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
