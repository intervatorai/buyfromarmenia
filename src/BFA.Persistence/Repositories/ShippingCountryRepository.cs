using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class ShippingCountryRepository : IShippingCountryRepository
{
    private readonly BfaDbContext _dbContext;

    public ShippingCountryRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ShippingCountry>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingCountries
            .AsNoTracking()
            .OrderBy(country => country.SortOrder)
            .ThenBy(country => country.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ShippingCountry>> GetEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingCountries
            .AsNoTracking()
            .Where(country => country.IsEnabled)
            .OrderBy(country => country.SortOrder)
            .ThenBy(country => country.NameEn)
            .ToListAsync(cancellationToken);
    }

    public Task<ShippingCountry?> GetByIsoCodeAsync(
        string isoCode,
        CancellationToken cancellationToken = default)
    {
        var code = isoCode.Trim().ToUpperInvariant();
        return _dbContext.ShippingCountries
            .AsNoTracking()
            .FirstOrDefaultAsync(country => country.IsoCode == code, cancellationToken);
    }

    public Task<ShippingCountry?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ShippingCountries
            .FirstOrDefaultAsync(country => country.Id == id, cancellationToken);
    }

    public async Task AddAsync(ShippingCountry country, CancellationToken cancellationToken = default)
    {
        await _dbContext.ShippingCountries.AddAsync(country, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        country.ClearDomainEvents();
    }

    public async Task AddRangeAsync(
        IReadOnlyList<ShippingCountry> countries,
        CancellationToken cancellationToken = default)
    {
        if (countries.Count == 0)
        {
            return;
        }

        await _dbContext.ShippingCountries.AddRangeAsync(countries, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var country in countries)
        {
            country.ClearDomainEvents();
        }
    }

    public async Task UpdateAsync(ShippingCountry country, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        country.ClearDomainEvents();
    }
}
