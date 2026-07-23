using BFA.Modules.Shipping.Domain.Aggregates;

namespace BFA.Modules.Shipping.Domain.Repositories;

public interface IShippingRateBracketRepository
{
    Task<IReadOnlyList<ShippingRateBracket>> GetByCountryAsync(
        string countryIsoCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShippingRateBracket>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ShippingRateBracket?> GetActiveForWeightAsync(
        string countryIsoCode,
        decimal weightKg,
        CancellationToken cancellationToken = default);

    Task<ShippingRateBracket?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(ShippingRateBracket bracket, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShippingRateBracket bracket, CancellationToken cancellationToken = default);
    Task DeleteAsync(ShippingRateBracket bracket, CancellationToken cancellationToken = default);
}
