using BFA.Modules.Inventory.Domain.Aggregates;
using BFA.Modules.Inventory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class StockItemRepository : IStockItemRepository
{
    private readonly BfaDbContext _dbContext;

    public StockItemRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StockItem?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithReservations()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<StockItem?> GetByVariantIdAsync(
        Guid productVariantId,
        CancellationToken cancellationToken = default)
    {
        return QueryWithReservations()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.ProductVariantId == productVariantId,
                cancellationToken);
    }

    public Task<StockItem?> GetByVariantIdForUpdateAsync(
        Guid productVariantId,
        CancellationToken cancellationToken = default)
    {
        return QueryWithReservations()
            .FirstOrDefaultAsync(
                item => item.ProductVariantId == productVariantId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<StockItem>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithReservations()
            .AsNoTracking()
            .Where(item => item.SupplierId == supplierId)
            .OrderBy(item => item.OnHand - item.Reserved)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        StockItem stockItem,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.StockItems.AddAsync(stockItem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        stockItem.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        StockItem stockItem,
        CancellationToken cancellationToken = default)
    {
        var persistedReservationIds = await _dbContext.Set<StockReservation>()
            .AsNoTracking()
            .Where(reservation => reservation.StockItemId == stockItem.Id)
            .Select(reservation => reservation.Id)
            .ToListAsync(cancellationToken);

        foreach (var reservation in stockItem.Reservations)
        {
            var entry = _dbContext.Entry(reservation);
            if (persistedReservationIds.Contains(reservation.Id) || entry.State == EntityState.Added)
            {
                continue;
            }

            if (entry.State is EntityState.Modified or EntityState.Unchanged)
            {
                entry.State = EntityState.Detached;
            }

            if (entry.State == EntityState.Detached)
            {
                _dbContext.Set<StockReservation>().Add(reservation);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        stockItem.ClearDomainEvents();
    }

    public async Task DeleteAsync(
        StockItem stockItem,
        CancellationToken cancellationToken = default)
    {
        _dbContext.StockItems.Remove(stockItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<StockItem> QueryWithReservations()
    {
        return _dbContext.StockItems.Include("_reservations");
    }
}
