using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Shipping.Domain.ValueObjects;

public sealed class CustomsDeclaration : ValueObject
{
    public string Description { get; private set; } = string.Empty;
    public string? HsCode { get; private set; }
    public decimal DeclaredValue { get; private set; }
    public string Currency { get; private set; } = "USD";

    private CustomsDeclaration()
    {
    }

    public CustomsDeclaration(
        string description,
        decimal declaredValue,
        string currency,
        string? hsCode = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Customs description is required.");
        }

        if (declaredValue <= 0)
        {
            throw new DomainException("Declared value must be positive.");
        }

        Description = description.Trim();
        DeclaredValue = declaredValue;
        Currency = currency.Trim().ToUpperInvariant();
        HsCode = string.IsNullOrWhiteSpace(hsCode) ? null : hsCode.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Description;
        yield return HsCode;
        yield return DeclaredValue;
        yield return Currency;
    }
}
