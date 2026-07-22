using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Warehouse;

public record GetEligibleInboundShipmentsQuery(Guid? CustomerOrderId = null)
    : IRequest<IReadOnlyList<EligibleInboundShipmentDto>>;

public record EligibleInboundShipmentDto(
    Guid Id,
    string ReferenceNumber,
    Guid CustomerOrderId,
    Guid SupplierOrderId,
    Guid SupplierId,
    int ItemsCount,
    decimal? WeightKg,
    DateTime CreatedAtUtc);

public sealed class GetEligibleInboundShipmentsQueryHandler
    : IRequestHandler<GetEligibleInboundShipmentsQuery, IReadOnlyList<EligibleInboundShipmentDto>>
{
    private readonly IInboundShipmentRepository _inboundShipmentRepository;

    public GetEligibleInboundShipmentsQueryHandler(
        IInboundShipmentRepository inboundShipmentRepository)
    {
        _inboundShipmentRepository = inboundShipmentRepository;
    }

    public async Task<IReadOnlyList<EligibleInboundShipmentDto>> Handle(
        GetEligibleInboundShipmentsQuery request,
        CancellationToken cancellationToken)
    {
        var shipments = await _inboundShipmentRepository.GetEligibleForConsolidationAsync(
            request.CustomerOrderId,
            cancellationToken);

        return shipments.Select(shipment => new EligibleInboundShipmentDto(
            shipment.Id,
            shipment.ReferenceNumber,
            shipment.CustomerOrderId,
            shipment.SupplierOrderId,
            shipment.SupplierId,
            shipment.ItemsCount,
            shipment.Receipt?.WeightKg,
            shipment.CreatedAtUtc)).ToList();
    }
}
