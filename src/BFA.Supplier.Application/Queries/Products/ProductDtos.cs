namespace BFA.Supplier.Application.Queries.Products;

public record SupplierProductDto(
    Guid Id,
    string Name,
    string ShortDescription,
    string Description,
    decimal Price,
    string Currency,
    string Status,
    Guid? CategoryId,
    int VariantsCount,
    int MediaCount,
    string? PrimaryImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProductDetailDto(
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
    string DefaultLanguage,
    Guid? CategoryId,
    ProductShippingDto? Shipping,
    IReadOnlyList<ProductTranslationDto> Translations,
    IReadOnlyList<ProductVariantDto> Variants,
    IReadOnlyList<ProductMediaDto> Media,
    IReadOnlyList<ProductDocumentDto> Documents,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProductTranslationDto(
    string LanguageCode,
    string Name,
    string ShortDescription,
    string Description);

public record ProductVariantDto(
    Guid Id,
    string SupplierSku,
    string? Size,
    string? Color,
    decimal Weight,
    string CountryOfOrigin,
    string Status);

public record ProductMediaDto(
    Guid Id,
    Guid MediaAssetId,
    string StorageKey,
    string Url,
    string? AltText,
    bool IsPrimary,
    int SortOrder);

public record ProductDocumentDto(
    Guid Id,
    string DocumentType,
    string FileName,
    string FileUrl);

public record ProductShippingDto(
    decimal NetWeight,
    decimal GrossWeight,
    decimal PackageLength,
    decimal PackageWidth,
    decimal PackageHeight,
    string PackageDimensionUnit,
    bool IsFragile,
    bool IsPerishable);

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    int SortOrder);
