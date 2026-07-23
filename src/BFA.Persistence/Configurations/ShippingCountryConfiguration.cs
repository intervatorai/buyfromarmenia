using BFA.Modules.Shipping.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class ShippingCountryConfiguration : IEntityTypeConfiguration<ShippingCountry>
{
    public static readonly Guid ArmeniaSeedId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public void Configure(EntityTypeBuilder<ShippingCountry> builder)
    {
        builder.ToTable("shipping_countries", "shipping");
        builder.HasKey(country => country.Id);
        builder.HasIndex(country => country.IsoCode).IsUnique();
        builder.HasIndex(country => country.IsEnabled);
        builder.HasIndex(country => country.SortOrder);

        builder.Property(country => country.IsoCode).HasMaxLength(2).IsRequired();
        builder.Property(country => country.NameEn).HasMaxLength(120).IsRequired();
        builder.Property(country => country.NameHy).HasMaxLength(120).IsRequired();
        builder.Property(country => country.IsEnabled).IsRequired();
        builder.Property(country => country.SortOrder).IsRequired();
        builder.Property(country => country.CreatedAtUtc).IsRequired();
        builder.Property(country => country.UpdatedAtUtc).IsRequired();

        builder.Ignore(country => country.DomainEvents);

        var seededAt = new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new
            {
                Id = ArmeniaSeedId,
                IsoCode = "AM",
                NameEn = "Armenia",
                NameHy = "Հայաստան",
                IsEnabled = true,
                SortOrder = 0,
                CreatedAtUtc = seededAt,
                UpdatedAtUtc = seededAt
            });
    }
}
