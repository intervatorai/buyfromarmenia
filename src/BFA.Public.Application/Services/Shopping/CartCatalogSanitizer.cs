using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Aggregates;

namespace BFA.Public.Application.Services.Shopping;

public static class CartCatalogSanitizer
{
    /// <summary>
    /// Drops cart lines whose product or variant no longer exists in the catalog.
    /// Returns how many lines were removed.
    /// </summary>
    public static async Task<int> RemoveUnavailableItemsAsync(
        ShoppingCart cart,
        IProductRepository productRepository,
        CancellationToken cancellationToken = default)
    {
        if (cart.Items.Count == 0)
        {
            return 0;
        }

        var productsById = new Dictionary<Guid, Product?>();
        foreach (var productId in cart.Items.Select(item => item.ProductId).Distinct())
        {
            productsById[productId] = await productRepository.GetByIdAsync(
                productId,
                cancellationToken);
        }

        var removed = 0;
        foreach (var item in cart.Items.ToList())
        {
            productsById.TryGetValue(item.ProductId, out var product);
            var variantOk = product?.Variants.Any(variant => variant.Id == item.ProductVariantId)
                ?? false;
            if (product is null || !variantOk)
            {
                cart.RemoveItem(item.Id);
                removed++;
            }
        }

        return removed;
    }
}
