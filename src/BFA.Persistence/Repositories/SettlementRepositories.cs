using BFA.Modules.Settlements.Domain.Aggregates;
using BFA.Modules.Settlements.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class SupplierSettlementRepository : ISupplierSettlementRepository
{
    private readonly BfaDbContext _dbContext;

    public SupplierSettlementRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SupplierSettlement>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SupplierSettlements
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierSettlement>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SupplierSettlements
            .AsNoTracking()
            .Where(s => s.SupplierId == supplierId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<SupplierSettlement?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SupplierSettlements
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        SupplierSettlement settlement,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SupplierSettlements.AddAsync(settlement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        settlement.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        SupplierSettlement settlement,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        settlement.ClearDomainEvents();
    }
}

public sealed class PayoutRepository : IPayoutRepository
{
    private readonly BfaDbContext _dbContext;

    public PayoutRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Payout>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payouts
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payout>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payouts
            .AsNoTracking()
            .Where(p => p.SupplierId == supplierId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Payout?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Payouts
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Payout payout, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payouts.AddAsync(payout, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        payout.ClearDomainEvents();
    }

    public async Task UpdateAsync(Payout payout, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        payout.ClearDomainEvents();
    }
}
