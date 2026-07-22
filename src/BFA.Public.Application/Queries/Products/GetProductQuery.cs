using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.Products;

public record GetProductQuery(string SlugOrId, string? Language = null)
    : IRequest<PublicProductDetailDto?>;

public record PublicProductDetailDto(
    Guid Id,
    string Slug,
    string Name,
    string ShortDescription,
    string Description,
    string Ingredients,
    string UsageInstructions,
    decimal Price,
    string Currency,
    Guid? CategoryId,
    string? CategorySlug,
    string? PrimaryImageUrl,
    IReadOnlyList<PublicProductImageDto> Images,
    IReadOnlyList<PublicProductVariantDto> Variants,
    PublicProductShippingDto? Shipping);

public record PublicProductImageDto(
    Guid Id,
    Guid MediaAssetId,
    string StorageKey,
    string Url,
    string? AltText,
    bool IsPrimary);

public record PublicProductVariantDto(
    Guid Id,
    string SupplierSku,
    string? Size,
    string? Color,
    decimal Weight,
    string CountryOfOrigin,
    int Available);

public record PublicProductShippingDto(
    decimal NetWeight,
    decimal GrossWeight,
    decimal PackageLength,
    decimal PackageWidth,
    decimal PackageHeight,
    bool IsFragile,
    bool IsPerishable);

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, PublicProductDetailDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IMediaUrlResolver _mediaUrlResolver;

    public GetProductQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IStockItemRepository stockItemRepository,
        IMediaUrlResolver mediaUrlResolver)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _stockItemRepository = stockItemRepository;
        _mediaUrlResolver = mediaUrlResolver;
    }

    public async Task<PublicProductDetailDto?> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var product = Guid.TryParse(request.SlugOrId, out var productId)
            ? await _productRepository.GetByIdAsync(productId, cancellationToken)
            : await _productRepository.GetBySlugAsync(request.SlugOrId, cancellationToken);

        if (product is null || product.Status != ProductStatus.Published)
        {
            return null;
        }

        var (name, description, shortDescription) = ProductDisplayHelper.GetDisplayText(
            product,
            request.Language);

        var translation = product.Translations.FirstOrDefault(t =>
                !string.IsNullOrWhiteSpace(request.Language)
                && string.Equals(
                    t.Language.Value,
                    request.Language,
                    StringComparison.OrdinalIgnoreCase))
            ?? product.Translations.FirstOrDefault(t =>
                t.Language.Value == product.DefaultLanguage)
            ?? product.Translations.FirstOrDefault();

        var primaryImage = product.Media.FirstOrDefault(m => m.IsPrimary)
            ?? product.Media.OrderBy(m => m.SortOrder).FirstOrDefault();
        var variants = await Task.WhenAll(product.Variants.Select(async variant =>
        {
            var stock = await _stockItemRepository.GetByVariantIdAsync(
                variant.Id,
                cancellationToken);
            return new PublicProductVariantDto(
                variant.Id,
                variant.SupplierSku,
                variant.Size,
                variant.Color,
                variant.Weight,
                variant.CountryOfOrigin,
                stock?.Available ?? 0);
        }));

        string? categorySlug = null;
        if (product.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(
                product.CategoryId.Value,
                cancellationToken);
            categorySlug = category?.Translations.FirstOrDefault()?.Slug;
        }

        return new PublicProductDetailDto(
            product.Id,
            product.Slug,
            name,
            shortDescription,
            description,
            translation?.Ingredients ?? string.Empty,
            translation?.UsageInstructions ?? string.Empty,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            product.CategoryId,
            categorySlug,
            primaryImage is null ? null : _mediaUrlResolver.Resolve(primaryImage.MediaAsset.StorageKey),
            product.Media.Select(m => new PublicProductImageDto(
                m.Id,
                m.MediaAssetId,
                m.MediaAsset.StorageKey,
                _mediaUrlResolver.Resolve(m.MediaAsset.StorageKey),
                m.AltText,
                m.IsPrimary)).ToList(),
            variants,
            product.ShippingProfile is null
                ? null
                : new PublicProductShippingDto(
                    product.ShippingProfile.NetWeight,
                    product.ShippingProfile.GrossWeight,
                    product.ShippingProfile.PackageLength,
                    product.ShippingProfile.PackageWidth,
                    product.ShippingProfile.PackageHeight,
                    product.ShippingProfile.IsFragile,
                    product.ShippingProfile.IsPerishable));
    }
}
