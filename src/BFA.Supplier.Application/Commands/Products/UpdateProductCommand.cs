using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Catalog.Domain.ValueObjects;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record UpdateProductCommand(
    Guid SupplierId,
    Guid ProductId,
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
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? PackageLength = null,
    decimal? PackageWidth = null,
    decimal? PackageHeight = null,
    bool IsFragile = false,
    bool IsPerishable = false) : IRequest<bool>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductSearchKeywordGenerator _keywordGenerator;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProductSearchKeywordGenerator keywordGenerator)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _keywordGenerator = keywordGenerator;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);

        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        var translations = CreateProductCommandHandler.NormalizeTranslations(request.Translations);
        var primary = translations.FirstOrDefault(t => t.LanguageCode == "en")
            ?? translations.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Name));
        if (primary is null || string.IsNullOrWhiteSpace(primary.Name))
        {
            return false;
        }

        var money = new Money(request.Price, request.Currency);

        foreach (var translation in translations.Where(t => !string.IsNullOrWhiteSpace(t.Name)))
        {
            product.UpdateDetails(
                money,
                translation.Name,
                translation.Description,
                translation.LanguageCode,
                translation.ShortDescription,
                request.Ingredients,
                request.UsageInstructions);
        }

        product.AssignCategory(request.CategoryId);

        var shouldUpsertVariant =
            !string.IsNullOrWhiteSpace(request.SupplierSku)
            || request.VariantWeight is > 0
            || product.Variants.Count == 0;

        if (shouldUpsertVariant)
        {
            var existing = product.Variants.FirstOrDefault();
            var weight = request.VariantWeight is > 0
                ? request.VariantWeight.Value
                : existing?.Weight ?? 0.5m;
            var sku = !string.IsNullOrWhiteSpace(request.SupplierSku)
                ? request.SupplierSku.Trim().ToUpperInvariant()
                : existing?.SupplierSku
                  ?? await ProductSkuAssigner.ResolveAsync(
                      null,
                      request.CategoryId ?? product.CategoryId,
                      _categoryRepository,
                      _productRepository,
                      cancellationToken);

            product.UpsertDefaultVariant(
                sku,
                weight,
                request.CountryOfOrigin,
                size: request.VariantSize,
                color: request.VariantColor);
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

        product.SetSlug(SlugHelper.From(primary.Name));
        await ProductSlugAssigner.AssignUniqueSlugAsync(product, _productRepository, cancellationToken);
        await ProductSearchIndexUpdater.RefreshAsync(product, _keywordGenerator, cancellationToken);

        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
