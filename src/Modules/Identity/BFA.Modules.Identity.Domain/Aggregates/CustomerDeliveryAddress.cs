using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Identity.Domain.Aggregates;

public sealed class CustomerDeliveryAddress : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Line1 { get; private set; } = string.Empty;
    public string? Line2 { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Region { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private CustomerDeliveryAddress()
    {
    }

    public static CustomerDeliveryAddress Create(
        Guid userId,
        string countryCode,
        string city,
        string line1,
        string? line2 = null,
        string? postalCode = null,
        string? region = null,
        string? label = null,
        bool isDefault = false)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        var address = new CustomerDeliveryAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsDefault = isDefault,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        address.ApplyFields(countryCode, city, line1, line2, postalCode, region, label);
        return address;
    }

    public void Update(
        string countryCode,
        string city,
        string line1,
        string? line2 = null,
        string? postalCode = null,
        string? region = null,
        string? label = null)
    {
        ApplyFields(countryCode, city, line1, line2, postalCode, region, label);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDefault()
    {
        IsDefault = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ClearDefault()
    {
        IsDefault = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Address ToAddress() =>
        new(CountryCode, City, Line1, Line2, PostalCode, Region);

    private void ApplyFields(
        string countryCode,
        string city,
        string line1,
        string? line2,
        string? postalCode,
        string? region,
        string? label)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Trim().Length != 2)
        {
            throw new DomainException("A two-letter delivery country code is required.");
        }

        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(line1))
        {
            throw new DomainException("Delivery city and address line are required.");
        }

        CountryCode = countryCode.Trim().ToUpperInvariant();
        City = city.Trim();
        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        PostalCode = string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim();
        Region = string.IsNullOrWhiteSpace(region) ? null : region.Trim();
        Label = string.IsNullOrWhiteSpace(label) ? "Home" : label.Trim();
    }
}
