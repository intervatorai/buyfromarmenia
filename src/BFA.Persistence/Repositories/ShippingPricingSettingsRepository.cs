using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class ShippingPricingSettingsRepository : IShippingPricingSettingsRepository
{
    private readonly BfaDbContext _dbContext;

    public ShippingPricingSettingsRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShippingPricingSettings> GetOrCreateAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await _dbContext.ShippingPricingSettings
            .FirstOrDefaultAsync(
                item => item.Id == ShippingPricingSettings.SingletonId,
                cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = ShippingPricingSettings.CreateDefault();
        await _dbContext.ShippingPricingSettings.AddAsync(settings, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        settings.ClearDomainEvents();
        return settings;
    }

    public async Task UpdateAsync(
        ShippingPricingSettings settings,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        settings.ClearDomainEvents();
    }
}
