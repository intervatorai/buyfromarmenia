using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Events;
using BFA.Modules.Catalog.Domain.ValueObjects;

namespace BFA.Modules.Catalog.Domain.Aggregates;

public sealed class Product : AggregateRoot
{
    public Guid SupplierId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }
    public ProductType ProductType { get; private set; } = ProductType.Standard;
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    public ProductTag Tag { get; private set; } = ProductTag.None;
    public Money BasePrice { get; private set; } = Money.Zero("USD");
    public string Slug { get; private set; } = string.Empty;
    public string? SearchKeywords { get; private set; }
    public string SearchText { get; private set; } = string.Empty;
    public string DefaultLanguage { get; private set; } = "en";
    public ShippingProfile? ShippingProfile { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ProductTranslation> _translations = [];
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductMedia> _media = [];
    private readonly List<ProductDocument> _documents = [];

    public IReadOnlyCollection<ProductTranslation> Translations => _translations.AsReadOnly();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyCollection<ProductMedia> Media => _media.AsReadOnly();
    public IReadOnlyCollection<ProductDocument> Documents => _documents.AsReadOnly();

    private Product()
    {
    }

    public static Product Create(
        Guid supplierId,
        Money basePrice,
        string name,
        string description,
        string languageCode = "en",
        Guid? categoryId = null,
        Guid? brandId = null,
        ProductType productType = ProductType.Standard,
        string shortDescription = "",
        string ingredients = "",
        string usageInstructions = "",
        string seoTitle = "",
        string seoDescription = "")
    {
        if (supplierId == Guid.Empty)
        {
            throw new DomainException("Supplier id is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        var language = LanguageCode.From(languageCode);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            CategoryId = categoryId,
            BrandId = brandId,
            ProductType = productType,
            BasePrice = basePrice,
            DefaultLanguage = language.Value,
            Status = ProductStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        product.SetSlug(SlugHelper.From(name));

        product._translations.Add(new ProductTranslation(
            product.Id,
            language,
            name.Trim(),
            description.Trim(),
            shortDescription,
            ingredients,
            usageInstructions,
            seoTitle,
            seoDescription));

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id, supplierId));
        return product;
    }

    public void SetSlug(string slug)
    {
        var normalized = SlugHelper.From(slug);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = $"product-{Id:N}";
        }

        Slug = normalized;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSearchKeywords(string? keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords))
        {
            SearchKeywords = null;
        }
        else
        {
            var normalized = string.Join(
                ", ",
                keywords
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(part => part.ToLowerInvariant())
                    .Where(part => part.Length > 1)
                    .Distinct(StringComparer.Ordinal)
                    .Take(40));

            SearchKeywords = normalized.Length <= 500
                ? normalized
                : normalized[..500].TrimEnd(',', ' ');
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RebuildSearchDocument()
    {
        var parts = _translations
            .SelectMany(translation => new[]
            {
                translation.Name,
                translation.ShortDescription,
                translation.Description,
                translation.Ingredients,
                translation.SeoTitle
            })
            .Concat(_variants.Select(variant => variant.SupplierSku))
            .Append(SearchKeywords ?? string.Empty)
            .Append(Slug)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim().ToLowerInvariant());

        var document = string.Join(' ', parts);
        SearchText = document.Length <= 4000 ? document : document[..4000];
        UpdatedAt = DateTime.UtcNow;
    }

    public ProductTranslation GetTranslation(LanguageCode language)
    {
        var translation = _translations.FirstOrDefault(t => t.Language == language);
        if (translation is null)
        {
            throw new DomainException($"Translation for language '{language}' was not found.");
        }

        return translation;
    }

    public void UpdateDetails(
        Money basePrice,
        string name,
        string description,
        string languageCode,
        string shortDescription = "",
        string ingredients = "",
        string usageInstructions = "",
        string seoTitle = "",
        string seoDescription = "")
    {
        EnsureEditable();

        var language = LanguageCode.From(languageCode);
        BasePrice = basePrice;
        UpdatedAt = DateTime.UtcNow;

        var translation = _translations.FirstOrDefault(t => t.Language == language);
        if (translation is null)
        {
            _translations.Add(new ProductTranslation(
                Id,
                language,
                name.Trim(),
                description.Trim(),
                shortDescription,
                ingredients,
                usageInstructions,
                seoTitle,
                seoDescription));
        }
        else
        {
            translation.Update(
                name.Trim(),
                description.Trim(),
                shortDescription,
                ingredients,
                usageInstructions,
                seoTitle,
                seoDescription);
        }

        if (Status == ProductStatus.Published)
        {
            Status = ProductStatus.PendingReview;
        }
    }

    public void AssignCategory(Guid? categoryId)
    {
        EnsureEditable();
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin override: edit without forcing re-review and without editable-status checks.
    /// </summary>
    public void UpdateDetailsAsAdmin(
        Money basePrice,
        string name,
        string description,
        string languageCode,
        string shortDescription = "",
        string ingredients = "",
        string usageInstructions = "",
        string seoTitle = "",
        string seoDescription = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        var language = LanguageCode.From(languageCode);
        BasePrice = basePrice;
        UpdatedAt = DateTime.UtcNow;

        var translation = _translations.FirstOrDefault(t => t.Language == language);
        if (translation is null)
        {
            _translations.Add(new ProductTranslation(
                Id,
                language,
                name.Trim(),
                description.Trim(),
                shortDescription,
                ingredients,
                usageInstructions,
                seoTitle,
                seoDescription));
        }
        else
        {
            translation.Update(
                name.Trim(),
                description.Trim(),
                shortDescription,
                ingredients,
                usageInstructions,
                seoTitle,
                seoDescription);
        }
    }

    public void AssignCategoryAsAdmin(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTag(ProductTag tag)
    {
        Tag = tag;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignBrand(Guid? brandId)
    {
        EnsureEditable();
        BrandId = brandId;
        UpdatedAt = DateTime.UtcNow;
    }

    public ProductVariant AddVariant(
        string supplierSku,
        decimal weight,
        string countryOfOrigin,
        string? barcode = null,
        string? size = null,
        string? color = null,
        Dimensions? dimensions = null,
        string? customsCode = null)
    {
        EnsureEditable();

        if (_variants.Any(v => v.SupplierSku.Equals(supplierSku, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"Variant with SKU '{supplierSku}' already exists.");
        }

        var variant = new ProductVariant(
            Id,
            supplierSku,
            weight,
            countryOfOrigin,
            barcode,
            size,
            color,
            dimensions,
            customsCode);

        _variants.Add(variant);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new VariantAddedDomainEvent(Id, variant.Id, variant.SupplierSku));
        return variant;
    }

    /// <summary>
    /// Updates the first (default) variant or creates it when the product has none yet.
    /// Used by the simple single-variant supplier form.
    /// </summary>
    public ProductVariant UpsertDefaultVariant(
        string supplierSku,
        decimal weight,
        string countryOfOrigin,
        string? size = null,
        string? color = null)
    {
        EnsureEditable();

        var existing = _variants.FirstOrDefault();
        if (existing is null)
        {
            return AddVariant(supplierSku, weight, countryOfOrigin, size: size, color: color);
        }

        existing.Update(
            supplierSku,
            weight,
            countryOfOrigin,
            existing.Barcode,
            size,
            color,
            existing.Dimensions,
            existing.CustomsCode);
        UpdatedAt = DateTime.UtcNow;
        return existing;
    }

    public ProductMedia AddMedia(
        MediaAsset mediaAsset,
        string? altText = null,
        int sortOrder = 0,
        bool isPrimary = false)
    {
        EnsureEditable();
        ArgumentNullException.ThrowIfNull(mediaAsset);

        if (_media.Any(item => item.MediaAssetId == mediaAsset.Id))
        {
            throw new DomainException("This media asset is already attached to the product.");
        }

        if (isPrimary || _media.Count == 0)
        {
            foreach (var item in _media.Where(m => m.IsPrimary))
            {
                item.SetPrimary(false);
            }

            isPrimary = true;
        }

        var media = new ProductMedia(Id, mediaAsset, altText, sortOrder, isPrimary);
        _media.Add(media);
        UpdatedAt = DateTime.UtcNow;
        return media;
    }

    public void SetPrimaryMedia(Guid productMediaId)
    {
        EnsureEditable();

        var target = _media.FirstOrDefault(item => item.Id == productMediaId)
            ?? throw new DomainException("Product media was not found.");

        foreach (var item in _media)
        {
            item.SetPrimary(item.Id == target.Id);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public ProductDocument AddDocument(
        ProductDocumentType documentType,
        string fileName,
        string fileUrl,
        DateTime? issuedAt = null,
        DateTime? expiresAt = null)
    {
        EnsureEditable();

        var document = new ProductDocument(Id, documentType, fileName, fileUrl, issuedAt, expiresAt);
        _documents.Add(document);
        UpdatedAt = DateTime.UtcNow;
        return document;
    }

    public void SetShippingProfile(ShippingProfile shippingProfile)
    {
        EnsureEditable();
        ShippingProfile = shippingProfile;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductShippingProfileChangedDomainEvent(Id, SupplierId));
    }

    public void SubmitForReview()
    {
        if (Status is not (ProductStatus.Draft or ProductStatus.ChangesRequested or ProductStatus.Rejected))
        {
            throw new DomainException("Product cannot be submitted for review in the current status.");
        }

        Status = ProductStatus.PendingReview;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductSubmittedForReviewDomainEvent(Id, SupplierId));
    }

    public void Approve()
    {
        if (Status != ProductStatus.PendingReview)
        {
            throw new DomainException("Only products pending review can be approved.");
        }

        Status = ProductStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductApprovedDomainEvent(Id, SupplierId));
    }

    public void Reject(string reason)
    {
        if (Status != ProductStatus.PendingReview)
        {
            throw new DomainException("Only products pending review can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Rejection reason is required.");
        }

        Status = ProductStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductRejectedDomainEvent(Id, SupplierId, reason));
    }

    public void RequestChanges(string reason)
    {
        if (Status != ProductStatus.PendingReview)
        {
            throw new DomainException("Only products pending review can be sent back for changes.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Change request reason is required.");
        }

        Status = ProductStatus.ChangesRequested;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status is not (ProductStatus.Approved or ProductStatus.Suspended))
        {
            throw new DomainException("Product cannot be published in the current status.");
        }

        Status = ProductStatus.Published;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ProductPublishedDomainEvent(Id, SupplierId));
    }

    public void Suspend()
    {
        if (Status != ProductStatus.Published)
        {
            throw new DomainException("Only published products can be suspended.");
        }

        Status = ProductStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureEditable()
    {
        if (Status is ProductStatus.Archived or ProductStatus.Suspended)
        {
            throw new DomainException("Product cannot be edited in the current status.");
        }
    }

    internal void LoadTranslations(IEnumerable<ProductTranslation> translations)
    {
        _translations.Clear();
        _translations.AddRange(translations);
    }

    internal void LoadVariants(IEnumerable<ProductVariant> variants)
    {
        _variants.Clear();
        _variants.AddRange(variants);
    }

    internal void LoadMedia(IEnumerable<ProductMedia> media)
    {
        _media.Clear();
        _media.AddRange(media);
    }

    internal void LoadDocuments(IEnumerable<ProductDocument> documents)
    {
        _documents.Clear();
        _documents.AddRange(documents);
    }
}
