using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Ordering.Domain.Enums;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Enums;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Orders;

public record SetAdminTrackingStageCommand(
    Guid OrderId,
    CustomerTrackingStage TrackingStage) : IRequest<SetAdminTrackingStageResult?>;

public record SetAdminTrackingStageResult(
    Guid OrderId,
    string TrackingStage,
    string Status,
    string PaymentStatus,
    string FulfillmentStatus,
    string? ShipmentStatus);

public sealed class SetAdminTrackingStageCommandHandler
    : IRequestHandler<SetAdminTrackingStageCommand, SetAdminTrackingStageResult?>
{
    private readonly ICustomerOrderRepository _orderRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IAuditLogger _auditLogger;

    public SetAdminTrackingStageCommandHandler(
        ICustomerOrderRepository orderRepository,
        IShipmentRepository shipmentRepository,
        IAuditLogger auditLogger)
    {
        _orderRepository = orderRepository;
        _shipmentRepository = shipmentRepository;
        _auditLogger = auditLogger;
    }

    public async Task<SetAdminTrackingStageResult?> Handle(
        SetAdminTrackingStageCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdForUpdateAsync(
            request.OrderId,
            cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Cannot update tracking for a cancelled order.");
        }

        var previous = order.TrackingStage.ToString();
        order.SetTrackingStageAsAdmin(request.TrackingStage);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        string? shipmentStatus = null;
        var shipment = await _shipmentRepository.GetByCustomerOrderIdAsync(
            order.Id,
            cancellationToken);
        if (shipment is not null)
        {
            var mapped = MapShipmentStatus(request.TrackingStage);
            if (mapped.HasValue && shipment.Status != mapped.Value)
            {
                // Reload for update if repository has separate for-update method
                var shipmentForUpdate = await _shipmentRepository.GetByIdForUpdateAsync(
                    shipment.Id,
                    cancellationToken);
                if (shipmentForUpdate is not null)
                {
                    try
                    {
                        shipmentForUpdate.SetStatus(mapped.Value);
                        await _shipmentRepository.UpdateAsync(shipmentForUpdate, cancellationToken);
                        shipmentStatus = shipmentForUpdate.Status.ToString();
                    }
                    catch (DomainException)
                    {
                        // Forward-only shipment rules: keep tracking stage even if shipment cannot move back.
                        shipmentStatus = shipmentForUpdate.Status.ToString();
                    }
                }
            }
            else
            {
                shipmentStatus = shipment.Status.ToString();
            }
        }

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "OrderTrackingStageUpdated",
            "CustomerOrder",
            order.Id,
            $"{{\"from\":\"{previous}\",\"to\":\"{order.TrackingStage}\"}}",
            cancellationToken);

        return new SetAdminTrackingStageResult(
            order.Id,
            order.TrackingStage.ToString(),
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.FulfillmentStatus.ToString(),
            shipmentStatus);
    }

    private static ShipmentStatus? MapShipmentStatus(CustomerTrackingStage stage) =>
        stage switch
        {
            CustomerTrackingStage.AtWarehouse => ShipmentStatus.Created,
            CustomerTrackingStage.Shipped => ShipmentStatus.PickedUp,
            CustomerTrackingStage.InTransit => ShipmentStatus.InTransit,
            CustomerTrackingStage.OutForDelivery => ShipmentStatus.OutForDelivery,
            CustomerTrackingStage.Delivered => ShipmentStatus.Delivered,
            _ => null
        };
}
