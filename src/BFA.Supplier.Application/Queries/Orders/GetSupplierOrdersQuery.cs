using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Orders;

public record GetSupplierOrdersQuery(Guid SupplierId)
    : IRequest<IReadOnlyList<SupplierOrderListItemDto>>;

public record SupplierOrderListItemDto(
    Guid Id,
    Guid CustomerOrderId,
    string Status,
    string? ShipmentStatus,
    string? TrackingNumber,
    decimal Subtotal,
    string Currency,
    int ItemsCount,
    DateTime CreatedAtUtc);

public sealed class GetSupplierOrdersQueryHandler
    : IRequestHandler<GetSupplierOrdersQuery, IReadOnlyList<SupplierOrderListItemDto>>
{
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public GetSupplierOrdersQueryHandler(
        ISupplierOrderRepository supplierOrderRepository,
        IShipmentRepository shipmentRepository)
    {
        _supplierOrderRepository = supplierOrderRepository;
        _shipmentRepository = shipmentRepository;
    }

    public async Task<IReadOnlyList<SupplierOrderListItemDto>> Handle(
        GetSupplierOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _supplierOrderRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);

        var shipments = await _shipmentRepository.GetAllAsync(cancellationToken);
        var shipmentByCustomerOrderId = shipments
            .GroupBy(shipment => shipment.CustomerOrderId)
            .ToDictionary(group => group.Key, group => group.First());

        return orders.Select(order =>
        {
            shipmentByCustomerOrderId.TryGetValue(order.CustomerOrderId, out var shipment);
            return new SupplierOrderListItemDto(
                order.Id,
                order.CustomerOrderId,
                order.Status.ToString(),
                shipment?.Status.ToString(),
                shipment?.TrackingNumber,
                order.Subtotal,
                order.Currency,
                order.Items.Count,
                order.CreatedAtUtc);
        }).ToList();
    }
}
