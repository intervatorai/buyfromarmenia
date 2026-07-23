using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class ShippingRateBracketRepository : IShippingRateBracketRepository
{
    private readonly BfaDbContext _dbContext;

    public ShippingRateBracketRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ShippingRateBracket>> GetByCountryAsync(
        string countryIsoCode,
        CancellationToken cancellationToken = default)
    {
        var code = countryIsoCode.Trim().ToUpperInvariant();
        return await _dbContext.ShippingRateBrackets
            .AsNoTracking()
            .Where(bracket => bracket.CountryIsoCode == code)
            .OrderBy(bracket => bracket.WeightFromKg)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ShippingRateBracket>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingRateBrackets
            .AsNoTracking()
            .OrderBy(bracket => bracket.CountryIsoCode)
            .ThenBy(bracket => bracket.WeightFromKg)
            .ToListAsync(cancellationToken);
    }

    public Task<ShippingRateBracket?> GetActiveForWeightAsync(
        string countryIsoCode,
        decimal weightKg,
        CancellationToken cancellationToken = default)
    {
        var code = countryIsoCode.Trim().ToUpperInvariant();
        return _dbContext.ShippingRateBrackets
            .AsNoTracking()
            .Where(bracket =>
                bracket.CountryIsoCode == code
                && bracket.IsActive
                && bracket.WeightFromKg <= weightKg
                && bracket.WeightToKg >= weightKg)
            .OrderBy(bracket => bracket.WeightFromKg)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ShippingRateBracket?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ShippingRateBrackets
            .FirstOrDefaultAsync(bracket => bracket.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        ShippingRateBracket bracket,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ShippingRateBrackets.AddAsync(bracket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        bracket.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        ShippingRateBracket bracket,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        bracket.ClearDomainEvents();
    }

    public async Task DeleteAsync(
        ShippingRateBracket bracket,
        CancellationToken cancellationToken = default)
    {
        _dbContext.ShippingRateBrackets.Remove(bracket);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
