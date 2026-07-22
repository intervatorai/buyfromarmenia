using BFA.BuildingBlocks.Application;
using BFA.Modules.Fulfillment.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Orders;

public record TransferSupplierOrderToWarehouseCommand(
    Guid SupplierId,
    Guid SupplierOrderId) : IRequest<bool>;

public sealed class TransferSupplierOrderToWarehouseCommandHandler
    : IRequestHandler<TransferSupplierOrderToWarehouseCommand, bool>
{
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly ISupplierOrderWarehouseTransferService _warehouseTransferService;

    public TransferSupplierOrderToWarehouseCommandHandler(
        ISupplierOrderRepository supplierOrderRepository,
        ISupplierOrderWarehouseTransferService warehouseTransferService)
    {
        _supplierOrderRepository = supplierOrderRepository;
        _warehouseTransferService = warehouseTransferService;
    }

    public async Task<bool> Handle(
        TransferSupplierOrderToWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _supplierOrderRepository.GetByIdAsync(
            request.SupplierOrderId,
            cancellationToken);

        if (order is null || order.SupplierId != request.SupplierId)
        {
            return false;
        }

        return await _warehouseTransferService.TransferReadyOrderToWarehouseAsync(
            request.SupplierOrderId,
            cancellationToken);
    }
}
