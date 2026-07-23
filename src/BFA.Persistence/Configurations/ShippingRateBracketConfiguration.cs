using BFA.Modules.Shipping.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class ShippingRateBracketConfiguration
    : IEntityTypeConfiguration<ShippingRateBracket>
{
    public void Configure(EntityTypeBuilder<ShippingRateBracket> builder)
    {
        builder.ToTable("shipping_rate_brackets", "shipping");
        builder.HasKey(bracket => bracket.Id);
        builder.HasIndex(bracket => bracket.CountryIsoCode);
        builder.HasIndex(bracket => new { bracket.CountryIsoCode, bracket.IsActive });

        builder.Property(bracket => bracket.CountryIsoCode).HasMaxLength(2).IsRequired();
        builder.Property(bracket => bracket.WeightFromKg).HasPrecision(18, 3).IsRequired();
        builder.Property(bracket => bracket.WeightToKg).HasPrecision(18, 3).IsRequired();
        builder.Property(bracket => bracket.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(bracket => bracket.Currency).HasMaxLength(3).IsRequired();
        builder.Property(bracket => bracket.IsActive).IsRequired();
        builder.Property(bracket => bracket.CreatedAtUtc).IsRequired();
        builder.Property(bracket => bracket.UpdatedAtUtc).IsRequired();
        builder.Ignore(bracket => bracket.DomainEvents);
    }
}

public sealed class ShippingPricingSettingsConfiguration
    : IEntityTypeConfiguration<ShippingPricingSettings>
{
    public void Configure(EntityTypeBuilder<ShippingPricingSettings> builder)
    {
        builder.ToTable("shipping_pricing_settings", "shipping");
        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.ErrorMarginPercent).HasPrecision(18, 2).IsRequired();
        builder.Property(settings => settings.UpdatedAtUtc).IsRequired();
        builder.Ignore(settings => settings.DomainEvents);

        var seededAt = new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new
            {
                Id = ShippingPricingSettings.SingletonId,
                ErrorMarginPercent = 10m,
                UpdatedAtUtc = seededAt
            });
    }
}
