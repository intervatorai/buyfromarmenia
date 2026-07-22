using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Shipping;

public record GetShipmentsQuery() : IRequest<IReadOnlyList<ShipmentListItemDto>>;

public record ShipmentListItemDto(
    Guid Id,
    string ReferenceNumber,
    Guid CustomerOrderId,
    Guid ConsolidationId,
    string Carrier,
    string TrackingNumber,
    string Status,
    decimal DeclaredValue,
    string Currency,
    DateTime CreatedAtUtc);

public sealed class GetShipmentsQueryHandler
    : IRequestHandler<GetShipmentsQuery, IReadOnlyList<ShipmentListItemDto>>
{
    private readonly IShipmentRepository _shipmentRepository;

    public GetShipmentsQueryHandler(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<IReadOnlyList<ShipmentListItemDto>> Handle(
        GetShipmentsQuery request,
        CancellationToken cancellationToken)
    {
        var shipments = await _shipmentRepository.GetAllAsync(cancellationToken);

        return shipments.Select(shipment => new ShipmentListItemDto(
            shipment.Id,
            shipment.ReferenceNumber,
            shipment.CustomerOrderId,
            shipment.ConsolidationId,
            shipment.Carrier.ToString(),
            shipment.TrackingNumber,
            shipment.Status.ToString(),
            shipment.Customs.DeclaredValue,
            shipment.Customs.Currency,
            shipment.CreatedAtUtc)).ToList();
    }
}
