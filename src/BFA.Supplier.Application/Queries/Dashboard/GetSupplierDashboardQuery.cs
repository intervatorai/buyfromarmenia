using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Fulfillment.Domain.Enums;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Dashboard;

public record GetSupplierDashboardQuery(Guid SupplierId) : IRequest<SupplierDashboardDto>;

public record SupplierDashboardDto(
    decimal SalesToday,
    decimal SalesThisMonth,
    int OrdersCount,
    int LowStockItemsCount,
    int PendingModerationCount,
    int AwaitingShipmentCount,
    int ReturnsCount,
    decimal EstimatedBalance,
    string Currency,
    IReadOnlyList<SupplierDashboardRecentOrderDto> RecentOrders);

public record SupplierDashboardRecentOrderDto(
    Guid Id,
    Guid CustomerOrderId,
    string Status,
    string? ShipmentStatus,
    decimal Subtotal,
    string Currency,
    int ItemsCount,
    DateTime CreatedAtUtc);

public sealed class GetSupplierDashboardQueryHandler
    : IRequestHandler<GetSupplierDashboardQuery, SupplierDashboardDto>
{
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public GetSupplierDashboardQueryHandler(
        ISupplierOrderRepository supplierOrderRepository,
        IStockItemRepository stockItemRepository,
        IProductRepository productRepository,
        IShipmentRepository shipmentRepository)
    {
        _supplierOrderRepository = supplierOrderRepository;
        _stockItemRepository = stockItemRepository;
        _productRepository = productRepository;
        _shipmentRepository = shipmentRepository;
    }

    public async Task<SupplierDashboardDto> Handle(
        GetSupplierDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _supplierOrderRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);
        var activeOrders = orders
            .Where(order => order.Status != SupplierOrderStatus.Cancelled)
            .ToList();

        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currency = activeOrders.FirstOrDefault()?.Currency ?? "USD";

        var salesToday = activeOrders
            .Where(order => order.CreatedAtUtc.Date == today)
            .Sum(order => order.Subtotal);
        var salesThisMonth = activeOrders
            .Where(order => order.CreatedAtUtc >= monthStart)
            .Sum(order => order.Subtotal);

        var stockItems = await _stockItemRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);
        var lowStockCount = stockItems.Count(item => item.Available <= item.LowStockThreshold);

        var products = await _productRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);
        var pendingModerationCount = products.Count(product =>
            product.Status is ProductStatus.PendingReview or ProductStatus.ChangesRequested);

        var awaitingShipmentCount = activeOrders.Count(order =>
            order.Status is SupplierOrderStatus.New
                or SupplierOrderStatus.Confirmed
                or SupplierOrderStatus.Preparing);

        var shipments = await _shipmentRepository.GetAllAsync(cancellationToken);
        var shipmentByCustomerOrderId = shipments
            .GroupBy(shipment => shipment.CustomerOrderId)
            .ToDictionary(group => group.Key, group => group.First());

        var recentOrders = activeOrders
            .OrderByDescending(order => order.CreatedAtUtc)
            .Take(5)
            .Select(order =>
            {
                shipmentByCustomerOrderId.TryGetValue(order.CustomerOrderId, out var shipment);
                return new SupplierDashboardRecentOrderDto(
                    order.Id,
                    order.CustomerOrderId,
                    order.Status.ToString(),
                    shipment?.Status.ToString(),
                    order.Subtotal,
                    order.Currency,
                    order.Items.Count,
                    order.CreatedAtUtc);
            })
            .ToList();

        return new SupplierDashboardDto(
            salesToday,
            salesThisMonth,
            activeOrders.Count,
            lowStockCount,
            pendingModerationCount,
            awaitingShipmentCount,
            ReturnsCount: 0,
            EstimatedBalance: Math.Round(salesThisMonth * 0.85m, 2),
            currency,
            recentOrders);
    }
}
