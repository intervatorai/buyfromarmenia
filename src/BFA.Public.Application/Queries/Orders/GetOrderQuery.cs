using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Payments.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.Orders;

public record GetOrderQuery(Guid OrderId, Guid CustomerUserId) : IRequest<PublicOrderDetailDto?>;

public record GetOrdersByCartQuery(Guid CartId) : IRequest<IReadOnlyList<PublicOrderSummaryDto>>;

public record GetOrdersByCustomerQuery(Guid CustomerUserId)
    : IRequest<IReadOnlyList<PublicOrderSummaryDto>>;

public record PublicOrderSummaryDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string PaymentStatus,
    string FulfillmentStatus,
    decimal Subtotal,
    string Currency,
    int ItemsCount,
    DateTime CreatedAtUtc);

public record PublicOrderDetailDto(
    Guid Id,
    string OrderNumber,
    string CustomerEmail,
    string CustomerFullName,
    string Status,
    string PaymentStatus,
    string? PaymentReference,
    string FulfillmentStatus,
    decimal Subtotal,
    string Currency,
    PublicOrderAddressDto ShippingAddress,
    IReadOnlyList<PublicOrderItemDto> Items,
    DateTime CreatedAtUtc);

public record PublicOrderAddressDto(
    string CountryCode,
    string City,
    string Line1,
    string? Line2,
    string? PostalCode,
    string? Region);

public record PublicOrderItemDto(
    Guid Id,
    string ProductName,
    string SupplierSku,
    string? ImageUrl,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);

public sealed class GetOrderQueryHandler
    : IRequestHandler<GetOrderQuery, PublicOrderDetailDto?>
{
    private readonly ICustomerOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetOrderQueryHandler(
        ICustomerOrderRepository orderRepository,
        IPaymentRepository paymentRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<PublicOrderDetailDto?> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.CustomerUserId != request.CustomerUserId)
        {
            return null;
        }

        var payment = await _paymentRepository.GetByCustomerOrderIdAsync(
            order.Id,
            cancellationToken);
        return OrderMapper.ToDetail(order, payment?.ExternalReference);
    }
}

public sealed class GetOrdersByCartQueryHandler
    : IRequestHandler<GetOrdersByCartQuery, IReadOnlyList<PublicOrderSummaryDto>>
{
    private readonly ICustomerOrderRepository _orderRepository;

    public GetOrdersByCartQueryHandler(ICustomerOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<PublicOrderSummaryDto>> Handle(
        GetOrdersByCartQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCartIdAsync(
            request.CartId,
            cancellationToken);
        return orders.Select(OrderMapper.ToSummary).ToList();
    }
}

public sealed class GetOrdersByCustomerQueryHandler
    : IRequestHandler<GetOrdersByCustomerQuery, IReadOnlyList<PublicOrderSummaryDto>>
{
    private readonly ICustomerOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(ICustomerOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<PublicOrderSummaryDto>> Handle(
        GetOrdersByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerUserIdAsync(
            request.CustomerUserId,
            cancellationToken);
        return orders.Select(OrderMapper.ToSummary).ToList();
    }
}

internal static class OrderMapper
{
    internal static PublicOrderSummaryDto ToSummary(
        BFA.Modules.Ordering.Domain.Aggregates.CustomerOrder order)
    {
        return new PublicOrderSummaryDto(
            order.Id,
            order.OrderNumber,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.FulfillmentStatus.ToString(),
            order.Subtotal,
            order.Currency,
            order.Items.Count,
            order.CreatedAtUtc);
    }

    internal static PublicOrderDetailDto ToDetail(
        BFA.Modules.Ordering.Domain.Aggregates.CustomerOrder order,
        string? paymentReference = null)
    {
        return new PublicOrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.CustomerEmail,
            order.CustomerFullName,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            paymentReference,
            order.FulfillmentStatus.ToString(),
            order.Subtotal,
            order.Currency,
            new PublicOrderAddressDto(
                order.ShippingAddress.CountryCode,
                order.ShippingAddress.City,
                order.ShippingAddress.Line1,
                order.ShippingAddress.Line2,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Region),
            order.Items.Select(item => new PublicOrderItemDto(
                item.Id,
                item.ProductName,
                item.SupplierSku,
                item.ImageUrl,
                item.UnitPrice,
                item.Currency,
                item.Quantity,
                item.LineTotal)).ToList(),
            order.CreatedAtUtc);
    }
}
