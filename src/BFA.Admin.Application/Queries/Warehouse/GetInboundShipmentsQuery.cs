using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Warehouse;

public record GetInboundShipmentsQuery(string? Status = null)
    : IRequest<IReadOnlyList<InboundShipmentListItemDto>>;

public record InboundShipmentListItemDto(
    Guid Id,
    string ReferenceNumber,
    Guid SupplierOrderId,
    Guid CustomerOrderId,
    Guid SupplierId,
    string? SupplierName,
    string Status,
    int ItemsCount,
    string? ScanReference,
    decimal? WeightKg,
    DateTime CreatedAtUtc,
    DateTime? ReceivedAtUtc);

public sealed class GetInboundShipmentsQueryHandler
    : IRequestHandler<GetInboundShipmentsQuery, IReadOnlyList<InboundShipmentListItemDto>>
{
    private readonly IInboundShipmentRepository _inboundShipmentRepository;
    private readonly ISupplierRepository _supplierRepository;

    public GetInboundShipmentsQueryHandler(
        IInboundShipmentRepository inboundShipmentRepository,
        ISupplierRepository supplierRepository)
    {
        _inboundShipmentRepository = inboundShipmentRepository;
        _supplierRepository = supplierRepository;
    }

    public async Task<IReadOnlyList<InboundShipmentListItemDto>> Handle(
        GetInboundShipmentsQuery request,
        CancellationToken cancellationToken)
    {
        InboundShipmentStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<InboundShipmentStatus>(request.Status, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var shipments = await _inboundShipmentRepository.GetAllAsync(status, cancellationToken);
        var suppliers = await _supplierRepository.GetAllAsync(cancellationToken);
        var supplierNames = suppliers.ToDictionary(
            supplier => supplier.Id,
            supplier => supplier.TradingName);

        return shipments.Select(shipment => new InboundShipmentListItemDto(
            shipment.Id,
            shipment.ReferenceNumber,
            shipment.SupplierOrderId,
            shipment.CustomerOrderId,
            shipment.SupplierId,
            supplierNames.GetValueOrDefault(shipment.SupplierId),
            shipment.Status.ToString(),
            shipment.ItemsCount,
            shipment.Receipt?.ScanReference,
            shipment.Receipt?.WeightKg,
            shipment.CreatedAtUtc,
            shipment.Receipt?.ReceivedAtUtc)).ToList();
    }
}
