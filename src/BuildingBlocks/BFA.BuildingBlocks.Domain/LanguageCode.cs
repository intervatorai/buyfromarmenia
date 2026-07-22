namespace BFA.BuildingBlocks.Domain;

public sealed class LanguageCode : ValueObject
{
    public static readonly LanguageCode English = new("en");
    public static readonly LanguageCode Armenian = new("hy");

    public string Value { get; }

    private LanguageCode(string value)
    {
        Value = value;
    }

    public static LanguageCode From(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();

        return normalized switch
        {
            "en" => English,
            "hy" => Armenian,
            _ => throw new DomainException($"Unsupported language code: {value}")
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
