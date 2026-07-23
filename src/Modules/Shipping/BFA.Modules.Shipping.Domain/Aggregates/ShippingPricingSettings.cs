using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Shipping.Domain.Aggregates;

public sealed class ShippingPricingSettings : AggregateRoot
{
    public static readonly Guid SingletonId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    public decimal ErrorMarginPercent { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ShippingPricingSettings()
    {
    }

    public static ShippingPricingSettings CreateDefault(decimal errorMarginPercent = 10m)
    {
        var settings = new ShippingPricingSettings
        {
            Id = SingletonId,
            UpdatedAtUtc = DateTime.UtcNow
        };
        settings.SetErrorMarginPercent(errorMarginPercent);
        return settings;
    }

    public void SetErrorMarginPercent(decimal errorMarginPercent)
    {
        if (errorMarginPercent < 0)
        {
            throw new DomainException("Error margin percent cannot be negative.");
        }

        if (errorMarginPercent > 100)
        {
            throw new DomainException("Error margin percent cannot exceed 100.");
        }

        ErrorMarginPercent = errorMarginPercent;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
