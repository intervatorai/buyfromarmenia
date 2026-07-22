using BFA.Modules.Inventory.Domain.Aggregates;

namespace BFA.Modules.Inventory.Domain.Repositories;

public interface IStockItemRepository
{
    Task<StockItem?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<StockItem?> GetByVariantIdAsync(
        Guid productVariantId,
        CancellationToken cancellationToken = default);

    Task<StockItem?> GetByVariantIdForUpdateAsync(
        Guid productVariantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockItem>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default);

    Task AddAsync(StockItem stockItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(StockItem stockItem, CancellationToken cancellationToken = default);
}
