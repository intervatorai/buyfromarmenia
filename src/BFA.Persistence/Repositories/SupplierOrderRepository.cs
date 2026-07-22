using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Fulfillment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class SupplierOrderRepository : ISupplierOrderRepository
{
    private readonly BfaDbContext _dbContext;

    public SupplierOrderRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SupplierOrder?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithItems()
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public Task<SupplierOrder?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithItems()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierOrder>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithItems()
            .AsNoTracking()
            .Where(order => order.SupplierId == supplierId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierOrder>> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithItems()
            .AsNoTracking()
            .Where(order => order.CustomerOrderId == customerOrderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierOrder>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await QueryWithItems()
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        SupplierOrder order,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SupplierOrders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        SupplierOrder order,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
    }

    private IQueryable<SupplierOrder> QueryWithItems()
    {
        return _dbContext.SupplierOrders.Include("_items");
    }
}
