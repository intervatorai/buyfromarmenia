namespace BFA.BuildingBlocks.Application;

public static class ProductCopyField
{
    public const string ShortDescription = "shortDescription";
    public const string Description = "description";

    public static bool IsValid(string? value) =>
        value is ShortDescription or Description;
}

public record ProductCopyRequest(
    string LanguageCode,
    string Field,
    string ProductName,
    string? ShortDescription,
    string? Description);

public record ProductCopyResult(string Text);

public interface IProductCopyGenerator
{
    bool IsEnabled { get; }

    Task<ProductCopyResult> GenerateAsync(
        ProductCopyRequest request,
        CancellationToken cancellationToken = default);
}
