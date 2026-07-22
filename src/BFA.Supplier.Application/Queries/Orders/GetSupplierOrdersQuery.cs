using BFA.Modules.Fulfillment.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Orders;

public record GetSupplierOrdersQuery(Guid SupplierId)
    : IRequest<IReadOnlyList<SupplierOrderListItemDto>>;

public record SupplierOrderListItemDto(
    Guid Id,
    Guid CustomerOrderId,
    string Status,
    decimal Subtotal,
    string Currency,
    int ItemsCount,
    DateTime CreatedAtUtc);

public sealed class GetSupplierOrdersQueryHandler
    : IRequestHandler<GetSupplierOrdersQuery, IReadOnlyList<SupplierOrderListItemDto>>
{
    private readonly ISupplierOrderRepository _supplierOrderRepository;

    public GetSupplierOrdersQueryHandler(ISupplierOrderRepository supplierOrderRepository)
    {
        _supplierOrderRepository = supplierOrderRepository;
    }

    public async Task<IReadOnlyList<SupplierOrderListItemDto>> Handle(
        GetSupplierOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _supplierOrderRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);

        return orders.Select(order => new SupplierOrderListItemDto(
            order.Id,
            order.CustomerOrderId,
            order.Status.ToString(),
            order.Subtotal,
            order.Currency,
            order.Items.Count,
            order.CreatedAtUtc)).ToList();
    }
}
