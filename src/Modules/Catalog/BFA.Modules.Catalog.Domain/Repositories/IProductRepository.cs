using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;

namespace BFA.Modules.Catalog.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchPublishedAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetBySupplierIdAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
    Task<string> AllocateNextSkuAsync(string prefix, CancellationToken cancellationToken = default);
    Task<bool> SupplierSkuExistsAsync(
        string supplierSku,
        Guid? excludeVariantId = null,
        CancellationToken cancellationToken = default);
}
