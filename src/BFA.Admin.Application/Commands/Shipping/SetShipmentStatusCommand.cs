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
        var order = await _customerOrderRepository.GetByIdForUpdateAsync(
            shipment.CustomerOrderId,
            cancellationToken);
        if (order is not null)
        {
            var tracking = MapTrackingStage(shipment.Status);
            if (order.TrackingStage != tracking)
            {
                order.SetTrackingStageAsAdmin(tracking);
            }

            if (shipment.Status == ShipmentStatus.Delivered)
            {
                order.MarkCompleted();
                orderCompleted = true;
            }

            await _customerOrderRepository.UpdateAsync(order, cancellationToken);
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

    private static BFA.Modules.Ordering.Domain.Enums.CustomerTrackingStage MapTrackingStage(
        ShipmentStatus status) =>
        status switch
        {
            ShipmentStatus.Created => BFA.Modules.Ordering.Domain.Enums.CustomerTrackingStage.AtWarehouse,
            ShipmentStatus.PickedUp => BFA.Modules.Ordering.Domain.Enums.CustomerTrackingStage.Shipped,
            ShipmentStatus.InTransit => BFA.Modules.Ordering.Domain.Enums.CustomerTrackingStage.InTransit,
            ShipmentStatus.OutForDelivery => BFA.Modules.Ordering.Domain.Enums.CustomerTrackingStage.OutForDelivery,
            ShipmentStatus.Delivered => BFA.Modules.Ordering.Domain.Enums.CustomerTrackingStage.Delivered,
            _ => BFA.Modules.Ordering.Domain.Enums.CustomerTrackingStage.AtWarehouse
        };
}
