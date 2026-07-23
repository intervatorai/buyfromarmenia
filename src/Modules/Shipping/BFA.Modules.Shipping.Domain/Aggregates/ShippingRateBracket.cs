using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Shipping.Domain.Aggregates;

public sealed class ShippingRateBracket : AggregateRoot
{
    public string CountryIsoCode { get; private set; } = string.Empty;
    public decimal WeightFromKg { get; private set; }
    public decimal WeightToKg { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ShippingRateBracket()
    {
    }

    public static ShippingRateBracket Create(
        string countryIsoCode,
        decimal weightFromKg,
        decimal weightToKg,
        decimal price,
        string currency = "USD",
        bool isActive = true)
    {
        var bracket = new ShippingRateBracket
        {
            Id = Guid.NewGuid(),
            IsActive = isActive,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        bracket.ApplyFields(countryIsoCode, weightFromKg, weightToKg, price, currency);
        return bracket;
    }

    public void Update(
        decimal weightFromKg,
        decimal weightToKg,
        decimal price,
        string currency,
        bool isActive)
    {
        ApplyFields(CountryIsoCode, weightFromKg, weightToKg, price, currency);
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool ContainsWeight(decimal weightKg) =>
        weightKg >= WeightFromKg && weightKg <= WeightToKg;

    public bool Overlaps(decimal weightFromKg, decimal weightToKg) =>
        WeightFromKg <= weightToKg && weightFromKg <= WeightToKg;

    private void ApplyFields(
        string countryIsoCode,
        decimal weightFromKg,
        decimal weightToKg,
        decimal price,
        string currency)
    {
        if (string.IsNullOrWhiteSpace(countryIsoCode) || countryIsoCode.Trim().Length != 2)
        {
            throw new DomainException("A two-letter ISO country code is required.");
        }

        if (weightFromKg < 0 || weightToKg < 0)
        {
            throw new DomainException("Weight bounds cannot be negative.");
        }

        if (weightToKg < weightFromKg)
        {
            throw new DomainException("Weight upper bound must be greater than or equal to lower bound.");
        }

        if (price < 0)
        {
            throw new DomainException("Shipping price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new DomainException("A three-letter currency code is required.");
        }

        CountryIsoCode = countryIsoCode.Trim().ToUpperInvariant();
        WeightFromKg = weightFromKg;
        WeightToKg = weightToKg;
        Price = price;
        Currency = currency.Trim().ToUpperInvariant();
    }
}
