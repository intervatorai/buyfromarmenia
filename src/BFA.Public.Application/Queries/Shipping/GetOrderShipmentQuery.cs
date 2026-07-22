using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.Shipping;

public record GetOrderShipmentQuery(Guid OrderId, Guid CustomerUserId)
    : IRequest<PublicShipmentDto?>;

public record PublicShipmentDto(
    string ReferenceNumber,
    string Carrier,
    string TrackingNumber,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed class GetOrderShipmentQueryHandler
    : IRequestHandler<GetOrderShipmentQuery, PublicShipmentDto?>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICustomerOrderRepository _orderRepository;

    public GetOrderShipmentQueryHandler(
        IShipmentRepository shipmentRepository,
        ICustomerOrderRepository orderRepository)
    {
        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<PublicShipmentDto?> Handle(
        GetOrderShipmentQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.CustomerUserId != request.CustomerUserId)
        {
            return null;
        }

        var shipment = await _shipmentRepository.GetByCustomerOrderIdAsync(
            request.OrderId,
            cancellationToken);

        return shipment is null
            ? null
            : new PublicShipmentDto(
                shipment.ReferenceNumber,
                shipment.Carrier.ToString(),
                shipment.TrackingNumber,
                shipment.Status.ToString(),
                shipment.CreatedAtUtc,
                shipment.UpdatedAtUtc);
    }
}
