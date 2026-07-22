using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Enums;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Shipping;

public record AdvanceShipmentStatusCommand(Guid ShipmentId) : IRequest<bool>;

public sealed class AdvanceShipmentStatusCommandHandler
    : IRequestHandler<AdvanceShipmentStatusCommand, bool>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerOrderRepository _customerOrderRepository;

    public AdvanceShipmentStatusCommandHandler(
        IShipmentRepository shipmentRepository,
        ICustomerOrderRepository customerOrderRepository)
    {
        _shipmentRepository = shipmentRepository;
        _customerOrderRepository = customerOrderRepository;
    }

    public async Task<bool> Handle(
        AdvanceShipmentStatusCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdForUpdateAsync(
            request.ShipmentId,
            cancellationToken);
        if (shipment is null)
        {
            return false;
        }

        shipment.AdvanceStatus();
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);

        if (shipment.Status == ShipmentStatus.Delivered)
        {
            var order = await _customerOrderRepository.GetByIdForUpdateAsync(
                shipment.CustomerOrderId,
                cancellationToken);

            if (order is not null)
            {
                order.MarkCompleted();
                await _customerOrderRepository.UpdateAsync(order, cancellationToken);
            }
        }

        return true;
    }
}
