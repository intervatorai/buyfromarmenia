using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Inventory;

public record GetSupplierStockQuery(Guid SupplierId)
    : IRequest<IReadOnlyList<SupplierStockDto>>;

public record SupplierStockDto(
    Guid Id,
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string SupplierSku,
    int OnHand,
    int Reserved,
    int Available,
    int LowStockThreshold,
    uint RowVersion);

public sealed class GetSupplierStockQueryHandler
    : IRequestHandler<GetSupplierStockQuery, IReadOnlyList<SupplierStockDto>>
{
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IProductRepository _productRepository;

    public GetSupplierStockQueryHandler(
        IStockItemRepository stockItemRepository,
        IProductRepository productRepository)
    {
        _stockItemRepository = stockItemRepository;
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<SupplierStockDto>> Handle(
        GetSupplierStockQuery request,
        CancellationToken cancellationToken)
    {
        var stockItems = await _stockItemRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);
        var products = await _productRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);

        return stockItems.Select(stock =>
        {
            var product = products.FirstOrDefault(item => item.Id == stock.ProductId);
            var variant = product?.Variants.FirstOrDefault(item =>
                item.Id == stock.ProductVariantId);
            var name = product?.Translations.FirstOrDefault()?.Name ?? "Product";

            return new SupplierStockDto(
                stock.Id,
                stock.ProductId,
                stock.ProductVariantId,
                name,
                variant?.SupplierSku ?? string.Empty,
                stock.OnHand,
                stock.Reserved,
                stock.Available,
                stock.LowStockThreshold,
                stock.RowVersion);
        }).ToList();
    }
}
