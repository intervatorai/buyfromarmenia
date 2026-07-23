using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Shipping.Domain.Aggregates;

public sealed class ShippingCountry : AggregateRoot
{
    public string IsoCode { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string NameHy { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; } = true;
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ShippingCountry()
    {
    }

    public static ShippingCountry Create(
        string isoCode,
        string nameEn,
        string nameHy,
        int sortOrder = 0,
        bool isEnabled = true)
    {
        var country = new ShippingCountry
        {
            Id = Guid.NewGuid(),
            IsEnabled = isEnabled,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        country.ApplyFields(isoCode, nameEn, nameHy, sortOrder);
        return country;
    }

    public void Update(string nameEn, string nameHy, int sortOrder)
    {
        ApplyNames(nameEn, nameHy);
        SortOrder = sortOrder;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void ApplyFields(string isoCode, string nameEn, string nameHy, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(isoCode) || isoCode.Trim().Length != 2)
        {
            throw new DomainException("A two-letter ISO country code is required.");
        }

        ApplyNames(nameEn, nameHy);
        IsoCode = isoCode.Trim().ToUpperInvariant();
        SortOrder = sortOrder;
    }

    private void ApplyNames(string nameEn, string nameHy)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
        {
            throw new DomainException("English country name is required.");
        }

        if (string.IsNullOrWhiteSpace(nameHy))
        {
            throw new DomainException("Armenian country name is required.");
        }

        NameEn = nameEn.Trim();
        NameHy = nameHy.Trim();
    }
}
