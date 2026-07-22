using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Catalog.Domain.ValueObjects;

public sealed class ShippingProfile : ValueObject
{
    public decimal NetWeight { get; }
    public decimal GrossWeight { get; }
    public decimal PackageLength { get; }
    public decimal PackageWidth { get; }
    public decimal PackageHeight { get; }
    public string PackageDimensionUnit { get; }
    public bool IsFragile { get; }
    public bool IsPerishable { get; }
    public bool RequiresCooling { get; }
    public bool ContainsLiquid { get; }
    public bool ContainsAlcohol { get; }
    public bool ContainsBattery { get; }
    public string? DangerousGoodsCode { get; }

    public ShippingProfile(
        decimal netWeight,
        decimal grossWeight,
        decimal packageLength,
        decimal packageWidth,
        decimal packageHeight,
        string packageDimensionUnit = "cm",
        bool isFragile = false,
        bool isPerishable = false,
        bool requiresCooling = false,
        bool containsLiquid = false,
        bool containsAlcohol = false,
        bool containsBattery = false,
        string? dangerousGoodsCode = null)
    {
        if (netWeight <= 0)
        {
            throw new DomainException("Net weight must be positive.");
        }

        if (grossWeight < netWeight)
        {
            throw new DomainException("Gross weight cannot be less than net weight.");
        }

        if (packageLength <= 0 || packageWidth <= 0 || packageHeight <= 0)
        {
            throw new DomainException("Package dimensions must be positive.");
        }

        NetWeight = netWeight;
        GrossWeight = grossWeight;
        PackageLength = packageLength;
        PackageWidth = packageWidth;
        PackageHeight = packageHeight;
        PackageDimensionUnit = packageDimensionUnit.Trim().ToLowerInvariant();
        IsFragile = isFragile;
        IsPerishable = isPerishable;
        RequiresCooling = requiresCooling;
        ContainsLiquid = containsLiquid;
        ContainsAlcohol = containsAlcohol;
        ContainsBattery = containsBattery;
        DangerousGoodsCode = dangerousGoodsCode?.Trim();
    }

    public Dimensions ToPackageDimensions() =>
        new(PackageLength, PackageWidth, PackageHeight, PackageDimensionUnit);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return NetWeight;
        yield return GrossWeight;
        yield return PackageLength;
        yield return PackageWidth;
        yield return PackageHeight;
        yield return PackageDimensionUnit;
        yield return IsFragile;
        yield return IsPerishable;
        yield return RequiresCooling;
        yield return ContainsLiquid;
        yield return ContainsAlcohol;
        yield return ContainsBattery;
        yield return DangerousGoodsCode;
    }
}
