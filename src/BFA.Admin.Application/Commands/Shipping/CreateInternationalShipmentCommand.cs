using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Enums;
using BFA.Modules.Shipping.Domain.Repositories;
using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Shipping;

public record CreateInternationalShipmentCommand(
    Guid ConsolidationId,
    string Carrier,
    string CustomsDescription) : IRequest<CreateInternationalShipmentResult?>;

public record CreateInternationalShipmentResult(
    Guid ShipmentId,
    string ReferenceNumber,
    string TrackingNumber);

public sealed class CreateInternationalShipmentCommandHandler
    : IRequestHandler<CreateInternationalShipmentCommand, CreateInternationalShipmentResult?>
{
    private readonly IConsolidationRepository _consolidationRepository;
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public CreateInternationalShipmentCommandHandler(
        IConsolidationRepository consolidationRepository,
        ICustomerOrderRepository customerOrderRepository,
        IShipmentRepository shipmentRepository)
    {
        _consolidationRepository = consolidationRepository;
        _customerOrderRepository = customerOrderRepository;
        _shipmentRepository = shipmentRepository;
    }

    public async Task<CreateInternationalShipmentResult?> Handle(
        CreateInternationalShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var consolidation = await _consolidationRepository.GetByIdAsync(
            request.ConsolidationId,
            cancellationToken);
        if (consolidation is null || consolidation.Status != ConsolidationStatus.Sealed)
        {
            return null;
        }

        var existingShipment = await _shipmentRepository.GetByConsolidationIdAsync(
            request.ConsolidationId,
            cancellationToken);
        if (existingShipment is not null)
        {
            return null;
        }

        var order = await _customerOrderRepository.GetByIdAsync(
            consolidation.CustomerOrderId,
            cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (!Enum.TryParse<ShippingCarrier>(request.Carrier, out var carrier))
        {
            carrier = ShippingCarrier.Stub;
        }

        var shipment = Shipment.CreateFromConsolidation(
            consolidation.Id,
            consolidation.CustomerOrderId,
            carrier,
            order.Subtotal,
            order.Currency,
            request.CustomsDescription);

        await _shipmentRepository.AddAsync(shipment, cancellationToken);

        return new CreateInternationalShipmentResult(
            shipment.Id,
            shipment.ReferenceNumber,
            shipment.TrackingNumber);
    }
}
