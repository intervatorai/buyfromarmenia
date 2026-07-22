using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Products;

public record GetProductQuery(Guid ProductId) : IRequest<AdminProductDetailDto?>;

public record AdminProductTranslationDto(
    string LanguageCode,
    string Name,
    string ShortDescription,
    string Description);

public record AdminProductDetailDto(
    Guid Id,
    Guid SupplierId,
    string Name,
    string ShortDescription,
    string Description,
    string Ingredients,
    string UsageInstructions,
    decimal Price,
    string Currency,
    string Status,
    string Tag,
    Guid? CategoryId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    AdminProductShippingDto? Shipping,
    IReadOnlyList<AdminProductTranslationDto> Translations,
    IReadOnlyList<AdminProductVariantDto> Variants,
    IReadOnlyList<AdminProductMediaDto> Media,
    IReadOnlyList<AdminProductDocumentDto> Documents);

public record AdminProductShippingDto(
    decimal NetWeight,
    decimal GrossWeight,
    decimal PackageLength,
    decimal PackageWidth,
    decimal PackageHeight,
    string PackageDimensionUnit,
    bool IsFragile,
    bool IsPerishable);

public record AdminProductVariantDto(
    Guid Id,
    string SupplierSku,
    string? Size,
    string? Color,
    decimal Weight,
    string CountryOfOrigin);

public record AdminProductMediaDto(
    Guid Id,
    Guid MediaAssetId,
    string StorageKey,
    string Url,
    bool IsPrimary,
    int SortOrder);

public record AdminProductDocumentDto(Guid Id, string FileName, string FileUrl, string DocumentType);

public sealed class GetProductQueryHandler : IRequestHandler<GetProductQuery, AdminProductDetailDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly IMediaUrlResolver _mediaUrlResolver;

    public GetProductQueryHandler(
        IProductRepository productRepository,
        IMediaUrlResolver mediaUrlResolver)
    {
        _productRepository = productRepository;
        _mediaUrlResolver = mediaUrlResolver;
    }

    public async Task<AdminProductDetailDto?> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var (name, description, shortDescription) = ProductDisplayHelper.GetDisplayText(product);
        var defaultTranslation = product.Translations.FirstOrDefault(t =>
            t.Language.Value == product.DefaultLanguage)
            ?? product.Translations.FirstOrDefault();

        return new AdminProductDetailDto(
            product.Id,
            product.SupplierId,
            name,
            shortDescription,
            description,
            defaultTranslation?.Ingredients ?? string.Empty,
            defaultTranslation?.UsageInstructions ?? string.Empty,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            product.Status.ToString(),
            product.Tag.ToString(),
            product.CategoryId,
            product.CreatedAt,
            product.UpdatedAt,
            product.ShippingProfile is null
                ? null
                : new AdminProductShippingDto(
                    product.ShippingProfile.NetWeight,
                    product.ShippingProfile.GrossWeight,
                    product.ShippingProfile.PackageLength,
                    product.ShippingProfile.PackageWidth,
                    product.ShippingProfile.PackageHeight,
                    product.ShippingProfile.PackageDimensionUnit,
                    product.ShippingProfile.IsFragile,
                    product.ShippingProfile.IsPerishable),
            product.Translations
                .OrderBy(translation => translation.Language.Value)
                .Select(translation => new AdminProductTranslationDto(
                    translation.Language.Value,
                    translation.Name,
                    translation.ShortDescription,
                    translation.Description))
                .ToList(),
            product.Variants.Select(variant => new AdminProductVariantDto(
                variant.Id,
                variant.SupplierSku,
                variant.Size,
                variant.Color,
                variant.Weight,
                variant.CountryOfOrigin)).ToList(),
            product.Media
                .OrderBy(media => media.SortOrder)
                .Select(media => new AdminProductMediaDto(
                    media.Id,
                    media.MediaAssetId,
                    media.MediaAsset.StorageKey,
                    _mediaUrlResolver.Resolve(media.MediaAsset.StorageKey),
                    media.IsPrimary,
                    media.SortOrder))
                .ToList(),
            product.Documents.Select(document => new AdminProductDocumentDto(
                document.Id,
                document.FileName,
                document.FileUrl,
                document.DocumentType.ToString())).ToList());
    }
}
