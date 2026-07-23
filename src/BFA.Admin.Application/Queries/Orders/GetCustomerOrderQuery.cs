using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Orders;

public record GetCustomerOrderQuery(Guid OrderId) : IRequest<AdminCustomerOrderDetailDto?>;

public record AdminCustomerOrderDetailDto(
    Guid Id,
    string OrderNumber,
    string CustomerEmail,
    string CustomerFullName,
    string Status,
    string PaymentStatus,
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
    DateTime CreatedAtUtc,
    AdminOrderAddressDto ShippingAddress,
    IReadOnlyList<AdminOrderItemDto> Items,
    IReadOnlyList<AdminSupplierOrderDto> SupplierOrders,
    AdminOrderShipmentDto? Shipment);

public record AdminOrderAddressDto(
    string CountryCode,
    string City,
    string Line1,
    string? Line2,
    string? PostalCode,
    string? Region);

public record AdminOrderItemDto(
    Guid ProductId,
    Guid ProductVariantId,
    Guid SupplierId,
    string ProductName,
    string SupplierSku,
    string? ImageUrl,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);

public record AdminSupplierOrderDto(
    Guid Id,
    Guid SupplierId,
    string Status,
    decimal Subtotal,
    string Currency,
    DateTime CreatedAtUtc,
    IReadOnlyList<AdminSupplierOrderItemDto> Items);

public record AdminSupplierOrderItemDto(
    Guid ProductId,
    string ProductName,
    string SupplierSku,
    decimal UnitPrice,
    string Currency,
    int Quantity);

public record AdminOrderShipmentDto(
    Guid Id,
    string ReferenceNumber,
    string Carrier,
    string TrackingNumber,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed class GetCustomerOrderQueryHandler
    : IRequestHandler<GetCustomerOrderQuery, AdminCustomerOrderDetailDto?>
{
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public GetCustomerOrderQueryHandler(
        ICustomerOrderRepository customerOrderRepository,
        ISupplierOrderRepository supplierOrderRepository,
        IShipmentRepository shipmentRepository)
    {
        _customerOrderRepository = customerOrderRepository;
        _supplierOrderRepository = supplierOrderRepository;
        _shipmentRepository = shipmentRepository;
    }

    public async Task<AdminCustomerOrderDetailDto?> Handle(
        GetCustomerOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _customerOrderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var supplierOrders = await _supplierOrderRepository.GetByCustomerOrderIdAsync(
            order.Id,
            cancellationToken);
        var shipment = await _shipmentRepository.GetByCustomerOrderIdAsync(
            order.Id,
            cancellationToken);

        return new AdminCustomerOrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.CustomerEmail,
            order.CustomerFullName,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
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
            order.CreatedAtUtc,
            new AdminOrderAddressDto(
                order.ShippingAddress.CountryCode,
                order.ShippingAddress.City,
                order.ShippingAddress.Line1,
                order.ShippingAddress.Line2,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Region),
            order.Items.Select(item => new AdminOrderItemDto(
                item.ProductId,
                item.ProductVariantId,
                item.SupplierId,
                item.ProductName,
                item.SupplierSku,
                item.ImageUrl,
                item.UnitPrice,
                item.Currency,
                item.Quantity,
                item.LineTotal)).ToList(),
            supplierOrders.Select(supplierOrder => new AdminSupplierOrderDto(
                supplierOrder.Id,
                supplierOrder.SupplierId,
                supplierOrder.Status.ToString(),
                supplierOrder.Subtotal,
                supplierOrder.Currency,
                supplierOrder.CreatedAtUtc,
                supplierOrder.Items.Select(item => new AdminSupplierOrderItemDto(
                    item.ProductId,
                    item.ProductName,
                    item.SupplierSku,
                    item.UnitPrice,
                    item.Currency,
                    item.Quantity)).ToList())).ToList(),
            shipment is null
                ? null
                : new AdminOrderShipmentDto(
                    shipment.Id,
                    shipment.ReferenceNumber,
                    shipment.Carrier.ToString(),
                    shipment.TrackingNumber,
                    shipment.Status.ToString(),
                    shipment.CreatedAtUtc,
                    shipment.UpdatedAtUtc));
    }
}
