namespace BFA.BuildingBlocks.Application;

public record ProductSearchTranslationInput(
    string LanguageCode,
    string Name,
    string ShortDescription = "",
    string Description = "");

public record ProductSearchKeywordRequest(
    IReadOnlyList<ProductSearchTranslationInput> Translations,
    string? CategoryName = null);

public interface IProductSearchKeywordGenerator
{
    Task<string> GenerateAsync(
        ProductSearchKeywordRequest request,
        CancellationToken cancellationToken = default);
}
