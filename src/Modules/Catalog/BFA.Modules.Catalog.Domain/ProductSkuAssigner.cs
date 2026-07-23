using BFA.Modules.Catalog.Domain.Repositories;

namespace BFA.Modules.Catalog.Domain;

public static class ProductSkuAssigner
{
    public const string FallbackPrefix = "PR";

    public static async Task<string> ResolveAsync(
        string? requestedSku,
        Guid? categoryId,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(requestedSku))
        {
            return requestedSku.Trim().ToUpperInvariant();
        }

        var prefix = FallbackPrefix;
        if (categoryId.HasValue)
        {
            var category = await categoryRepository.GetByIdAsync(categoryId.Value, cancellationToken);
            if (category is not null && !string.IsNullOrWhiteSpace(category.SkuPrefix))
            {
                prefix = category.SkuPrefix;
            }
        }

        return await productRepository.AllocateNextSkuAsync(prefix, cancellationToken);
    }
}
