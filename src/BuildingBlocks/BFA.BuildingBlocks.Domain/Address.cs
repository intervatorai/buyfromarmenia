namespace BFA.BuildingBlocks.Domain;

public sealed class Address : ValueObject
{
    public string CountryCode { get; }
    public string City { get; }
    public string Line1 { get; }
    public string? Line2 { get; }
    public string? PostalCode { get; }
    public string? Region { get; }

    public Address(
        string countryCode,
        string city,
        string line1,
        string? line2 = null,
        string? postalCode = null,
        string? region = null)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            throw new DomainException("Country code is required.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainException("City is required.");
        }

        if (string.IsNullOrWhiteSpace(line1))
        {
            throw new DomainException("Address line 1 is required.");
        }

        CountryCode = countryCode.ToUpperInvariant();
        City = city.Trim();
        Line1 = line1.Trim();
        Line2 = line2?.Trim();
        PostalCode = postalCode?.Trim();
        Region = region?.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return City;
        yield return Line1;
        yield return Line2;
        yield return PostalCode;
        yield return Region;
    }
}
