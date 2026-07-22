using BFA.Modules.Fulfillment.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Orders;

public record UpdateSupplierOrderStatusCommand(
    Guid SupplierId,
    Guid SupplierOrderId,
    string Status) : IRequest<bool>;

public sealed class UpdateSupplierOrderStatusCommandHandler
    : IRequestHandler<UpdateSupplierOrderStatusCommand, bool>
{
    private readonly ISupplierOrderRepository _supplierOrderRepository;

    public UpdateSupplierOrderStatusCommandHandler(
        ISupplierOrderRepository supplierOrderRepository)
    {
        _supplierOrderRepository = supplierOrderRepository;
    }

    public async Task<bool> Handle(
        UpdateSupplierOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _supplierOrderRepository.GetByIdForUpdateAsync(
            request.SupplierOrderId,
            cancellationToken);

        if (order is null || order.SupplierId != request.SupplierId)
        {
            return false;
        }

        switch (request.Status)
        {
            case "Confirmed":
                order.Confirm();
                break;
            case "Preparing":
                order.MarkPreparing();
                break;
            case "ReadyForPickup":
                order.MarkReadyForPickup();
                break;
            default:
                return false;
        }

        await _supplierOrderRepository.UpdateAsync(order, cancellationToken);
        return true;
    }
}
