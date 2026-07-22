using BFA.Modules.Catalog.Domain.Repositories;

namespace BFA.Modules.Catalog.Domain;

public static class ProductSlugAssigner
{
    public static async Task AssignUniqueSlugAsync(
        Aggregates.Product product,
        IProductRepository productRepository,
        CancellationToken cancellationToken = default)
    {
        var baseSlug = string.IsNullOrWhiteSpace(product.Slug)
            ? SlugHelper.From(ProductDisplayHelper.GetDisplayText(product).Name)
            : product.Slug;

        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = $"product-{product.Id:N}";
        }

        var uniqueSlug = baseSlug;
        var suffix = 2;
        while (await productRepository.SlugExistsAsync(
                   uniqueSlug,
                   product.Id,
                   cancellationToken))
        {
            uniqueSlug = $"{baseSlug}-{suffix}";
            suffix++;
            if (suffix > 1000)
            {
                uniqueSlug = $"product-{product.Id:N}";
                break;
            }
        }

        product.SetSlug(uniqueSlug);
    }
}
