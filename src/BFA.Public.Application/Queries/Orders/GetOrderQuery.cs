using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Fulfillment.Domain.Repositories;
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
    decimal ShippingFee,
    decimal Total,
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
    string TrackingStage,
    decimal Subtotal,
    decimal EstimatedWeightKg,
    decimal ShippingFeeQuoted,
    decimal ShippingMarginPercent,
    decimal ShippingFee,
    decimal Total,
    string? ShippingAdjustmentReason,
    string Currency,
    PublicOrderAddressDto ShippingAddress,
    IReadOnlyList<PublicOrderItemDto> Items,
    IReadOnlyList<PublicSupplierFulfillmentDto> SupplierFulfillments,
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

public record PublicSupplierFulfillmentDto(
    string Status,
    int ItemsCount,
    IReadOnlyList<string> ProductNames);

public sealed class GetOrderQueryHandler
    : IRequestHandler<GetOrderQuery, PublicOrderDetailDto?>
{
    private readonly ICustomerOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISupplierOrderRepository _supplierOrderRepository;

    public GetOrderQueryHandler(
        ICustomerOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        ISupplierOrderRepository supplierOrderRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _supplierOrderRepository = supplierOrderRepository;
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
        var supplierOrders = await _supplierOrderRepository.GetByCustomerOrderIdAsync(
            order.Id,
            cancellationToken);
        return OrderMapper.ToDetail(order, payment?.ExternalReference, supplierOrders);
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
            order.ShippingFee,
            order.Total,
            order.Currency,
            order.Items.Count,
            order.CreatedAtUtc);
    }

    internal static PublicOrderDetailDto ToDetail(
        BFA.Modules.Ordering.Domain.Aggregates.CustomerOrder order,
        string? paymentReference = null,
        IReadOnlyList<SupplierOrder>? supplierOrders = null)
    {
        var fulfillments = (supplierOrders ?? Array.Empty<SupplierOrder>())
            .OrderBy(supplierOrder => supplierOrder.CreatedAtUtc)
            .Select(supplierOrder => new PublicSupplierFulfillmentDto(
                supplierOrder.Status.ToString(),
                supplierOrder.Items.Count,
                supplierOrder.Items
                    .Select(item => item.ProductName)
                    .Distinct()
                    .ToList()))
            .ToList();

        return new PublicOrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.CustomerEmail,
            order.CustomerFullName,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            paymentReference,
            order.FulfillmentStatus.ToString(),
            order.TrackingStage.ToString(),
            order.Subtotal,
            order.EstimatedWeightKg,
            order.ShippingFeeQuoted,
            order.ShippingMarginPercent,
            order.ShippingFee,
            order.Total,
            order.ShippingAdjustmentReason,
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
            fulfillments,
            order.CreatedAtUtc);
    }
}
