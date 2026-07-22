using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Enums;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Dashboard;

public record GetAdminDashboardQuery() : IRequest<AdminDashboardDto>;

public record AdminDashboardDto(
    int PendingReviewCount,
    int PublishedProductsCount,
    int ActiveSellersCount,
    int OrdersTodayCount);

public sealed class GetAdminDashboardQueryHandler
    : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ICustomerOrderRepository _customerOrderRepository;

    public GetAdminDashboardQueryHandler(
        IProductRepository productRepository,
        ISupplierRepository supplierRepository,
        ICustomerOrderRepository customerOrderRepository)
    {
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _customerOrderRepository = customerOrderRepository;
    }

    public async Task<AdminDashboardDto> Handle(
        GetAdminDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var pendingReview = await _productRepository.GetByStatusAsync(
            ProductStatus.PendingReview,
            cancellationToken);
        var changesRequested = await _productRepository.GetByStatusAsync(
            ProductStatus.ChangesRequested,
            cancellationToken);
        var publishedProducts = await _productRepository.GetByStatusAsync(
            ProductStatus.Published,
            cancellationToken);
        var activeSellers = await _supplierRepository.GetByStatusAsync(
            SupplierStatus.Active,
            cancellationToken);
        var orders = await _customerOrderRepository.GetAllAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var ordersTodayCount = orders.Count(order => order.CreatedAtUtc.Date == today);

        return new AdminDashboardDto(
            pendingReview.Count + changesRequested.Count,
            publishedProducts.Count,
            activeSellers.Count,
            ordersTodayCount);
    }
}
