using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Orders;

public record GetCustomerOrdersQuery : IRequest<IReadOnlyList<AdminCustomerOrderDto>>;

public record AdminCustomerOrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerEmail,
    string CustomerFullName,
    string Status,
    string PaymentStatus,
    string FulfillmentStatus,
    decimal Subtotal,
    string Currency,
    int ItemsCount,
    int SupplierOrdersCount,
    DateTime CreatedAtUtc);

public sealed class GetCustomerOrdersQueryHandler
    : IRequestHandler<GetCustomerOrdersQuery, IReadOnlyList<AdminCustomerOrderDto>>
{
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly ISupplierOrderRepository _supplierOrderRepository;

    public GetCustomerOrdersQueryHandler(
        ICustomerOrderRepository customerOrderRepository,
        ISupplierOrderRepository supplierOrderRepository)
    {
        _customerOrderRepository = customerOrderRepository;
        _supplierOrderRepository = supplierOrderRepository;
    }

    public async Task<IReadOnlyList<AdminCustomerOrderDto>> Handle(
        GetCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _customerOrderRepository.GetAllAsync(cancellationToken);
        var supplierOrders = await _supplierOrderRepository.GetAllAsync(cancellationToken);
        var supplierCounts = supplierOrders
            .GroupBy(order => order.CustomerOrderId)
            .ToDictionary(group => group.Key, group => group.Count());

        return orders.Select(order => new AdminCustomerOrderDto(
            order.Id,
            order.OrderNumber,
            order.CustomerEmail,
            order.CustomerFullName,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.FulfillmentStatus.ToString(),
            order.Subtotal,
            order.Currency,
            order.Items.Count,
            supplierCounts.GetValueOrDefault(order.Id),
            order.CreatedAtUtc)).ToList();
    }
}
