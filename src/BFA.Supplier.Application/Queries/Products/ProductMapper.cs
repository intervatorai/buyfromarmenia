using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;

namespace BFA.Supplier.Application.Queries.Products;

internal static class ProductMapper
{
    public static SupplierProductDto ToListItem(Product product, IMediaUrlResolver mediaUrlResolver)
    {
        var (name, description, shortDescription) = ProductDisplayHelper.GetDisplayText(product);
        var primaryImage = GetPrimaryMedia(product);

        return new SupplierProductDto(
            product.Id,
            name,
            shortDescription,
            description,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            product.Status.ToString(),
            product.CategoryId,
            product.Variants.Count,
            product.Media.Count,
            ResolveUrl(primaryImage, mediaUrlResolver),
            product.CreatedAt,
            product.UpdatedAt);
    }

    public static ProductDetailDto ToDetail(Product product, IMediaUrlResolver mediaUrlResolver)
    {
        var (name, description, shortDescription) = ProductDisplayHelper.GetDisplayText(product);
        var translation = product.Translations.FirstOrDefault(t =>
            t.Language.Value == product.DefaultLanguage)
            ?? product.Translations.FirstOrDefault();

        return new ProductDetailDto(
            product.Id,
            product.SupplierId,
            name,
            shortDescription,
            description,
            translation?.Ingredients ?? string.Empty,
            translation?.UsageInstructions ?? string.Empty,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            product.Status.ToString(),
            product.DefaultLanguage,
            product.CategoryId,
            MapShipping(product.ShippingProfile),
            product.Translations
                .OrderBy(item => item.Language.Value)
                .Select(item => new ProductTranslationDto(
                    item.Language.Value,
                    item.Name,
                    item.ShortDescription,
                    item.Description))
                .ToList(),
            product.Variants.Select(v => new ProductVariantDto(
                v.Id,
                v.SupplierSku,
                v.Size,
                v.Color,
                v.Weight,
                v.CountryOfOrigin,
                v.Status.ToString())).ToList(),
            product.Media.Select(m => new ProductMediaDto(
                m.Id,
                m.MediaAssetId,
                m.MediaAsset.StorageKey,
                mediaUrlResolver.Resolve(m.MediaAsset.StorageKey),
                m.AltText,
                m.IsPrimary,
                m.SortOrder)).ToList(),
            product.Documents.Select(d => new ProductDocumentDto(
                d.Id,
                d.DocumentType.ToString(),
                d.FileName,
                d.FileUrl)).ToList(),
            product.CreatedAt,
            product.UpdatedAt);
    }

    private static ProductMedia? GetPrimaryMedia(Product product) =>
        product.Media.FirstOrDefault(m => m.IsPrimary)
        ?? product.Media.OrderBy(m => m.SortOrder).FirstOrDefault();

    private static string? ResolveUrl(ProductMedia? media, IMediaUrlResolver mediaUrlResolver) =>
        media is null ? null : mediaUrlResolver.Resolve(media.MediaAsset.StorageKey);

    private static ProductShippingDto? MapShipping(
        BFA.Modules.Catalog.Domain.ValueObjects.ShippingProfile? profile)
    {
        if (profile is null)
        {
            return null;
        }

        return new ProductShippingDto(
            profile.NetWeight,
            profile.GrossWeight,
            profile.PackageLength,
            profile.PackageWidth,
            profile.PackageHeight,
            profile.PackageDimensionUnit,
            profile.IsFragile,
            profile.IsPerishable);
    }
}
