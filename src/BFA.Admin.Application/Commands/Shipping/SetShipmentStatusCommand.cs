using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Enums;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Shipping;

public record SetShipmentStatusCommand(
    Guid ShipmentId,
    ShipmentStatus Status) : IRequest<SetShipmentStatusResult?>;

public record SetShipmentStatusResult(
    Guid ShipmentId,
    string Status,
    Guid CustomerOrderId,
    bool OrderCompleted);

public sealed class SetShipmentStatusCommandHandler
    : IRequestHandler<SetShipmentStatusCommand, SetShipmentStatusResult?>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly IAuditLogger _auditLogger;

    public SetShipmentStatusCommandHandler(
        IShipmentRepository shipmentRepository,
        ICustomerOrderRepository customerOrderRepository,
        IAuditLogger auditLogger)
    {
        _shipmentRepository = shipmentRepository;
        _customerOrderRepository = customerOrderRepository;
        _auditLogger = auditLogger;
    }

    public async Task<SetShipmentStatusResult?> Handle(
        SetShipmentStatusCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdForUpdateAsync(
            request.ShipmentId,
            cancellationToken);
        if (shipment is null)
        {
            return null;
        }

        var previous = shipment.Status.ToString();
        shipment.SetStatus(request.Status);
        await _shipmentRepository.UpdateAsync(shipment, cancellationToken);

        var orderCompleted = false;
        if (shipment.Status == ShipmentStatus.Delivered)
        {
            var order = await _customerOrderRepository.GetByIdForUpdateAsync(
                shipment.CustomerOrderId,
                cancellationToken);
            if (order is not null)
            {
                order.MarkCompleted();
                await _customerOrderRepository.UpdateAsync(order, cancellationToken);
                orderCompleted = true;
            }
        }

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ShipmentStatusUpdated",
            "Shipment",
            shipment.Id,
            $"{{\"from\":\"{previous}\",\"to\":\"{shipment.Status}\"}}",
            cancellationToken);

        return new SetShipmentStatusResult(
            shipment.Id,
            shipment.Status.ToString(),
            shipment.CustomerOrderId,
            orderCompleted);
    }
}
