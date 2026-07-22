using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain.Aggregates;

namespace BFA.Admin.Application.Commands.Products;

internal static class ProductSearchIndexUpdater
{
    public static async Task RefreshAsync(
        Product product,
        IProductSearchKeywordGenerator keywordGenerator,
        CancellationToken cancellationToken = default)
    {
        var request = new ProductSearchKeywordRequest(
            product.Translations
                .Select(translation => new ProductSearchTranslationInput(
                    translation.Language.Value,
                    translation.Name,
                    translation.ShortDescription,
                    translation.Description))
                .ToList());

        var keywords = await keywordGenerator.GenerateAsync(request, cancellationToken);
        product.SetSearchKeywords(keywords);
        product.RebuildSearchDocument();
    }
}
