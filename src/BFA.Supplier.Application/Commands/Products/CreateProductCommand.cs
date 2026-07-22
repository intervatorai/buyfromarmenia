using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Catalog.Domain.ValueObjects;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record ProductTranslationInput(
    string LanguageCode,
    string Name,
    string ShortDescription = "",
    string Description = "");

public record CreateProductCommand(
    Guid SupplierId,
    decimal Price,
    string Currency,
    IReadOnlyList<ProductTranslationInput> Translations,
    Guid? CategoryId = null,
    string Ingredients = "",
    string UsageInstructions = "",
    string? SupplierSku = null,
    decimal? VariantWeight = null,
    string? VariantSize = null,
    string? VariantColor = null,
    string CountryOfOrigin = "AM",
    string? ImageStorageKey = null,
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? PackageLength = null,
    decimal? PackageWidth = null,
    decimal? PackageHeight = null,
    bool IsFragile = false,
    bool IsPerishable = false) : IRequest<Guid?>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid?>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductSearchKeywordGenerator _keywordGenerator;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IProductSearchKeywordGenerator keywordGenerator)
    {
        _productRepository = productRepository;
        _keywordGenerator = keywordGenerator;
    }

    public async Task<Guid?> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var translations = NormalizeTranslations(request.Translations);
        var primary = translations.FirstOrDefault(t => t.LanguageCode == "en")
            ?? translations.FirstOrDefault();
        if (primary is null || string.IsNullOrWhiteSpace(primary.Name))
        {
            return null;
        }

        var product = Product.Create(
            request.SupplierId,
            new Money(request.Price, request.Currency),
            primary.Name,
            primary.Description,
            primary.LanguageCode,
            request.CategoryId,
            shortDescription: primary.ShortDescription,
            ingredients: request.Ingredients,
            usageInstructions: request.UsageInstructions);

        foreach (var translation in translations.Where(t => t.LanguageCode != primary.LanguageCode))
        {
            if (string.IsNullOrWhiteSpace(translation.Name))
            {
                continue;
            }

            product.UpdateDetails(
                product.BasePrice,
                translation.Name,
                translation.Description,
                translation.LanguageCode,
                translation.ShortDescription,
                request.Ingredients,
                request.UsageInstructions);
        }

        if (!string.IsNullOrWhiteSpace(request.SupplierSku) && request.VariantWeight.HasValue)
        {
            product.AddVariant(
                request.SupplierSku,
                request.VariantWeight.Value,
                request.CountryOfOrigin,
                size: request.VariantSize,
                color: request.VariantColor);
        }

        if (!string.IsNullOrWhiteSpace(request.ImageStorageKey))
        {
            var asset = MediaAsset.Create(request.ImageStorageKey);
            product.AddMedia(asset, isPrimary: true);
        }

        if (request.NetWeight.HasValue
            && request.GrossWeight.HasValue
            && request.PackageLength.HasValue
            && request.PackageWidth.HasValue
            && request.PackageHeight.HasValue)
        {
            product.SetShippingProfile(new ShippingProfile(
                request.NetWeight.Value,
                request.GrossWeight.Value,
                request.PackageLength.Value,
                request.PackageWidth.Value,
                request.PackageHeight.Value,
                isFragile: request.IsFragile,
                isPerishable: request.IsPerishable));
        }

        await ProductSlugAssigner.AssignUniqueSlugAsync(product, _productRepository, cancellationToken);
        await ProductSearchIndexUpdater.RefreshAsync(product, _keywordGenerator, cancellationToken);
        await _productRepository.AddAsync(product, cancellationToken);
        return product.Id;
    }

    internal static IReadOnlyList<ProductTranslationInput> NormalizeTranslations(
        IReadOnlyList<ProductTranslationInput>? translations)
    {
        if (translations is null || translations.Count == 0)
        {
            return [];
        }

        return translations
            .Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode))
            .GroupBy(t => t.LanguageCode.Trim().ToLowerInvariant())
            .Select(group =>
            {
                var item = group.First();
                return item with
                {
                    LanguageCode = group.Key,
                    Name = item.Name?.Trim() ?? string.Empty,
                    ShortDescription = item.ShortDescription?.Trim() ?? string.Empty,
                    Description = item.Description?.Trim() ?? string.Empty
                };
            })
            .ToList();
    }
}
