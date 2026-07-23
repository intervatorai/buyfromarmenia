using BFA.Modules.Shipping.Domain.Aggregates;

namespace BFA.Modules.Shipping.Domain.Repositories;

public interface IShippingCountryRepository
{
    Task<IReadOnlyList<ShippingCountry>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShippingCountry>> GetEnabledAsync(
        CancellationToken cancellationToken = default);

    Task<ShippingCountry?> GetByIsoCodeAsync(
        string isoCode,
        CancellationToken cancellationToken = default);

    Task<ShippingCountry?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(ShippingCountry country, CancellationToken cancellationToken = default);
    Task AddRangeAsync(
        IReadOnlyList<ShippingCountry> countries,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(ShippingCountry country, CancellationToken cancellationToken = default);
}
