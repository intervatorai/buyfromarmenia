using BFA.Modules.Compliance.Domain.Aggregates;
using BFA.Modules.Compliance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class TradeRestrictionRepository : ITradeRestrictionRepository
{
    private readonly BfaDbContext _dbContext;

    public TradeRestrictionRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TradeRestriction>> GetActiveForCountryAsync(
        string destinationCountryCode,
        CancellationToken cancellationToken = default)
    {
        var countryCode = destinationCountryCode.Trim().ToUpperInvariant();

        return await _dbContext.TradeRestrictions
            .AsNoTracking()
            .Where(restriction =>
                restriction.IsActive
                && restriction.DestinationCountryCode == countryCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TradeRestriction>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TradeRestrictions
            .AsNoTracking()
            .OrderByDescending(restriction => restriction.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<TradeRestriction?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TradeRestrictions
            .FirstOrDefaultAsync(restriction => restriction.Id == id, cancellationToken);
    }

    public async Task AddAsync(TradeRestriction restriction, CancellationToken cancellationToken = default)
    {
        await _dbContext.TradeRestrictions.AddAsync(restriction, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        restriction.ClearDomainEvents();
    }

    public async Task UpdateAsync(TradeRestriction restriction, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        restriction.ClearDomainEvents();
    }
}
