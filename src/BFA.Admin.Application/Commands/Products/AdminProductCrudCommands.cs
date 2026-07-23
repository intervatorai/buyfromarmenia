using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Catalog.Domain.ValueObjects;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Products;

public record ProductTranslationInput(
    string LanguageCode,
    string Name,
    string ShortDescription = "",
    string Description = "");

public record CreateAdminProductCommand(
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
    bool PublishImmediately = false,
    ProductTag Tag = ProductTag.None) : IRequest<Guid?>;

public sealed class CreateAdminProductCommandHandler
    : IRequestHandler<CreateAdminProductCommand, Guid?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductSearchKeywordGenerator _keywordGenerator;
    private readonly IAuditLogger _auditLogger;
    private readonly IOutboxStore _outboxStore;

    public CreateAdminProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISupplierRepository supplierRepository,
        IProductSearchKeywordGenerator keywordGenerator,
        IAuditLogger auditLogger,
        IOutboxStore outboxStore)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _supplierRepository = supplierRepository;
        _keywordGenerator = keywordGenerator;
        _auditLogger = auditLogger;
        _outboxStore = outboxStore;
    }

    public async Task<Guid?> Handle(
        CreateAdminProductCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return null;
        }

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

            product.UpdateDetailsAsAdmin(
                product.BasePrice,
                translation.Name,
                translation.Description,
                translation.LanguageCode,
                translation.ShortDescription,
                request.Ingredients,
                request.UsageInstructions);
        }

        var weight = request.VariantWeight is > 0 ? request.VariantWeight.Value : 0.5m;
        var sku = await ProductSkuAssigner.ResolveAsync(
            request.SupplierSku,
            request.CategoryId,
            _categoryRepository,
            _productRepository,
            cancellationToken);
        product.AddVariant(
            sku,
            weight,
            request.CountryOfOrigin,
            size: request.VariantSize,
            color: request.VariantColor);

        if (!string.IsNullOrWhiteSpace(request.ImageStorageKey))
        {
            var asset = MediaAsset.Create(request.ImageStorageKey);
            product.AddMedia(asset, isPrimary: true);
        }

        product.SetTag(request.Tag);
        await ProductSlugAssigner.AssignUniqueSlugAsync(product, _productRepository, cancellationToken);
        await ProductSearchIndexUpdater.RefreshAsync(product, _keywordGenerator, cancellationToken);

        if (request.PublishImmediately)
        {
            product.SubmitForReview();
            product.Approve();
            product.Publish();
        }

        await _productRepository.AddAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            request.PublishImmediately ? "ProductCreatedAndPublished" : "ProductCreated",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);

        if (request.PublishImmediately)
        {
            await _outboxStore.EnqueueAsync(
                IntegrationEventTypes.ProductApproved,
                $"{{\"productId\":\"{product.Id}\",\"supplierId\":\"{product.SupplierId}\"}}",
                cancellationToken);
        }

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

public record UpdateAdminProductCommand(
    Guid ProductId,
    decimal Price,
    string Currency,
    IReadOnlyList<ProductTranslationInput> Translations,
    Guid? CategoryId = null,
    string Ingredients = "",
    string UsageInstructions = "",
    ProductTag Tag = ProductTag.None,
    string? SupplierSku = null,
    decimal? VariantWeight = null,
    string? VariantSize = null,
    string? VariantColor = null,
    string CountryOfOrigin = "AM") : IRequest<bool>;

public sealed class UpdateAdminProductCommandHandler
    : IRequestHandler<UpdateAdminProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductSearchKeywordGenerator _keywordGenerator;
    private readonly IAuditLogger _auditLogger;

    public UpdateAdminProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProductSearchKeywordGenerator keywordGenerator,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _keywordGenerator = keywordGenerator;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        UpdateAdminProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return false;
        }

        var translations = CreateAdminProductCommandHandler.NormalizeTranslations(request.Translations);
        var primary = translations.FirstOrDefault(t => t.LanguageCode == "en")
            ?? translations.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Name));
        if (primary is null || string.IsNullOrWhiteSpace(primary.Name))
        {
            return false;
        }

        var money = new Money(request.Price, request.Currency);

        foreach (var translation in translations.Where(t => !string.IsNullOrWhiteSpace(t.Name)))
        {
            product.UpdateDetailsAsAdmin(
                money,
                translation.Name,
                translation.Description,
                translation.LanguageCode,
                translation.ShortDescription,
                request.Ingredients,
                request.UsageInstructions);
        }

        product.AssignCategoryAsAdmin(request.CategoryId);
        product.SetTag(request.Tag);
        product.SetSlug(SlugHelper.From(primary.Name));
        await ProductSlugAssigner.AssignUniqueSlugAsync(product, _productRepository, cancellationToken);

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

            if (existing is null)
            {
                product.AddVariantAsAdmin(
                    sku,
                    weight,
                    request.CountryOfOrigin,
                    size: request.VariantSize,
                    color: request.VariantColor);
            }
            else
            {
                product.UpdateVariantAsAdmin(
                    existing.Id,
                    sku,
                    weight,
                    request.CountryOfOrigin,
                    size: request.VariantSize,
                    color: request.VariantColor);
            }
        }

        // Refresh search text after variant SKU changes so the index includes the new code.
        await ProductSearchIndexUpdater.RefreshAsync(product, _keywordGenerator, cancellationToken);

        await _productRepository.UpdateAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductUpdated",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record UpdateAdminProductVariantCommand(
    Guid ProductId,
    Guid VariantId,
    string SupplierSku,
    decimal Weight,
    string CountryOfOrigin,
    string? Size = null,
    string? Color = null) : IRequest<bool>;

public sealed class UpdateAdminProductVariantCommandHandler
    : IRequestHandler<UpdateAdminProductVariantCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;

    public UpdateAdminProductVariantCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        UpdateAdminProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SupplierSku))
        {
            throw new DomainException("Supplier SKU is required.");
        }

        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.UpdateVariantAsAdmin(
            request.VariantId,
            request.SupplierSku.Trim().ToUpperInvariant(),
            request.Weight,
            request.CountryOfOrigin,
            request.Size,
            request.Color);

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductVariantUpdated",
            "ProductVariant",
            request.VariantId,
            cancellationToken: cancellationToken);
        return true;
    }
}

public record AddAdminProductVariantCommand(
    Guid ProductId,
    string? SupplierSku,
    decimal Weight,
    string CountryOfOrigin,
    string? Size = null,
    string? Color = null) : IRequest<Guid?>;

public sealed class AddAdminProductVariantCommandHandler
    : IRequestHandler<AddAdminProductVariantCommand, Guid?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditLogger _auditLogger;

    public AddAdminProductVariantCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Guid?> Handle(
        AddAdminProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return null;
        }

        var sku = await ProductSkuAssigner.ResolveAsync(
            request.SupplierSku,
            product.CategoryId,
            _categoryRepository,
            _productRepository,
            cancellationToken);

        var variant = product.AddVariantAsAdmin(
            sku,
            request.Weight,
            request.CountryOfOrigin,
            request.Size,
            request.Color);

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductVariantAdded",
            "ProductVariant",
            variant.Id,
            cancellationToken: cancellationToken);
        return variant.Id;
    }
}

public record DeleteAdminProductVariantCommand(
    Guid ProductId,
    Guid VariantId) : IRequest<bool>;

public sealed class DeleteAdminProductVariantCommandHandler
    : IRequestHandler<DeleteAdminProductVariantCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IAuditLogger _auditLogger;

    public DeleteAdminProductVariantCommandHandler(
        IProductRepository productRepository,
        IStockItemRepository stockItemRepository,
        IShoppingCartRepository cartRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _stockItemRepository = stockItemRepository;
        _cartRepository = cartRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        DeleteAdminProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return false;
        }

        await ProductVariantRemoval.EnsureStockClearAsync(
            _stockItemRepository,
            request.VariantId,
            cancellationToken);

        product.RemoveVariantAsAdmin(request.VariantId);
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _cartRepository.RemoveItemsByProductVariantIdAsync(
            request.VariantId,
            cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductVariantDeleted",
            "ProductVariant",
            request.VariantId,
            cancellationToken: cancellationToken);
        return true;
    }
}

public record ClearAdminProductShippingCommand(Guid ProductId) : IRequest<bool>;

public sealed class ClearAdminProductShippingCommandHandler
    : IRequestHandler<ClearAdminProductShippingCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;

    public ClearAdminProductShippingCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        ClearAdminProductShippingCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.ClearShippingProfileAsAdmin();
        await _productRepository.UpdateAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductShippingCleared",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);
        return true;
    }
}

public record SetAdminProductShippingCommand(
    Guid ProductId,
    decimal NetWeight,
    decimal GrossWeight,
    decimal PackageLength,
    decimal PackageWidth,
    decimal PackageHeight,
    string PackageDimensionUnit = "cm",
    bool IsFragile = false,
    bool IsPerishable = false) : IRequest<bool>;

public sealed class SetAdminProductShippingCommandHandler
    : IRequestHandler<SetAdminProductShippingCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;

    public SetAdminProductShippingCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        SetAdminProductShippingCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.SetShippingProfileAsAdmin(new ShippingProfile(
            request.NetWeight,
            request.GrossWeight,
            request.PackageLength,
            request.PackageWidth,
            request.PackageHeight,
            request.PackageDimensionUnit,
            request.IsFragile,
            request.IsPerishable));

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductShippingUpdated",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);
        return true;
    }
}

internal static class ProductVariantRemoval
{
    public static async Task EnsureStockClearAsync(
        IStockItemRepository stockItemRepository,
        Guid variantId,
        CancellationToken cancellationToken)
    {
        var stock = await stockItemRepository.GetByVariantIdForUpdateAsync(
            variantId,
            cancellationToken);
        if (stock is null)
        {
            return;
        }

        if (stock.Reserved > 0)
        {
            throw new DomainException(
                "Cannot delete this variant while stock is reserved for open orders.");
        }

        await stockItemRepository.DeleteAsync(stock, cancellationToken);
    }
}

public record DeleteAdminProductCommand(Guid ProductId) : IRequest<bool>;

public sealed class DeleteAdminProductCommandHandler
    : IRequestHandler<DeleteAdminProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;

    public DeleteAdminProductCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        DeleteAdminProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null)
        {
            return false;
        }

        var productId = product.Id;
        await _productRepository.DeleteAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductDeleted",
            "Product",
            productId,
            cancellationToken: cancellationToken);

        return true;
    }
}
