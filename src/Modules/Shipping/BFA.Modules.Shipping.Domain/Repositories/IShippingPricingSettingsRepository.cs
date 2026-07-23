using BFA.Modules.Shipping.Domain.Aggregates;

namespace BFA.Modules.Shipping.Domain.Repositories;

public interface IShippingPricingSettingsRepository
{
    Task<ShippingPricingSettings> GetOrCreateAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(ShippingPricingSettings settings, CancellationToken cancellationToken = default);
}
