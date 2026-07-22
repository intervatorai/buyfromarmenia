using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Catalog.Domain.ValueObjects;
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
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductSearchKeywordGenerator _keywordGenerator;
    private readonly IAuditLogger _auditLogger;
    private readonly IOutboxStore _outboxStore;

    public CreateAdminProductCommandHandler(
        IProductRepository productRepository,
        ISupplierRepository supplierRepository,
        IProductSearchKeywordGenerator keywordGenerator,
        IAuditLogger auditLogger,
        IOutboxStore outboxStore)
    {
        _productRepository = productRepository;
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
    ProductTag Tag = ProductTag.None) : IRequest<bool>;

public sealed class UpdateAdminProductCommandHandler
    : IRequestHandler<UpdateAdminProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductSearchKeywordGenerator _keywordGenerator;
    private readonly IAuditLogger _auditLogger;

    public UpdateAdminProductCommandHandler(
        IProductRepository productRepository,
        IProductSearchKeywordGenerator keywordGenerator,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
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
