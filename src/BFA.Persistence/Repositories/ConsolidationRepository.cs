using BFA.Modules.Warehouse.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class ConsolidationRepository : IConsolidationRepository
{
    private readonly BfaDbContext _dbContext;

    public ConsolidationRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Consolidation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(consolidation => consolidation.Id == id, cancellationToken);
    }

    public Task<Consolidation?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .FirstOrDefaultAsync(consolidation => consolidation.Id == id, cancellationToken);
    }

    public Task<Consolidation?> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                consolidation => consolidation.CustomerOrderId == customerOrderId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Consolidation>> GetAllAsync(
        ConsolidationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = QueryWithDetails().AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(consolidation => consolidation.Status == status.Value);
        }

        return await query
            .OrderByDescending(consolidation => consolidation.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Consolidation consolidation,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Consolidations.AddAsync(consolidation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        consolidation.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        Consolidation consolidation,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        consolidation.ClearDomainEvents();
    }

    private IQueryable<Consolidation> QueryWithDetails()
    {
        return _dbContext.Consolidations
            .Include("_items")
            .Include("_packages");
    }
}
