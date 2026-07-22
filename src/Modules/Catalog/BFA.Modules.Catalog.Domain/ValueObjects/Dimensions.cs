using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Catalog.Domain.ValueObjects;

public sealed class Dimensions : ValueObject
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public string Unit { get; }

    public Dimensions(decimal length, decimal width, decimal height, string unit = "cm")
    {
        if (length <= 0 || width <= 0 || height <= 0)
        {
            throw new DomainException("Dimensions must be positive.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new DomainException("Dimension unit is required.");
        }

        Length = length;
        Width = width;
        Height = height;
        Unit = unit.Trim().ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Length;
        yield return Width;
        yield return Height;
        yield return Unit;
    }
}
