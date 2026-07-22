using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.ValueObjects;

namespace BFA.Modules.Catalog.Domain.Aggregates;

public sealed class ProductVariant : Entity
{
    public Guid ProductId { get; private set; }
    public string SupplierSku { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public string? Size { get; private set; }
    public string? Color { get; private set; }
    public decimal Weight { get; private set; }
    public Dimensions? Dimensions { get; private set; }
    public string? CustomsCode { get; private set; }
    public string CountryOfOrigin { get; private set; } = "AM";
    public ProductVariantStatus Status { get; private set; } = ProductVariantStatus.Active;

    private ProductVariant()
    {
    }

    internal ProductVariant(
        Guid productId,
        string supplierSku,
        decimal weight,
        string countryOfOrigin,
        string? barcode = null,
        string? size = null,
        string? color = null,
        Dimensions? dimensions = null,
        string? customsCode = null)
    {
        if (string.IsNullOrWhiteSpace(supplierSku))
        {
            throw new DomainException("Supplier SKU is required.");
        }

        if (weight <= 0)
        {
            throw new DomainException("Variant weight must be positive.");
        }

        if (string.IsNullOrWhiteSpace(countryOfOrigin))
        {
            throw new DomainException("Country of origin is required.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        SupplierSku = supplierSku.Trim();
        Weight = weight;
        CountryOfOrigin = countryOfOrigin.Trim().ToUpperInvariant();
        Barcode = barcode?.Trim();
        Size = size?.Trim();
        Color = color?.Trim();
        Dimensions = dimensions;
        CustomsCode = customsCode?.Trim();
    }

    internal void Update(
        string supplierSku,
        decimal weight,
        string countryOfOrigin,
        string? barcode,
        string? size,
        string? color,
        Dimensions? dimensions,
        string? customsCode)
    {
        if (string.IsNullOrWhiteSpace(supplierSku))
        {
            throw new DomainException("Supplier SKU is required.");
        }

        if (weight <= 0)
        {
            throw new DomainException("Variant weight must be positive.");
        }

        SupplierSku = supplierSku.Trim();
        Weight = weight;
        CountryOfOrigin = countryOfOrigin.Trim().ToUpperInvariant();
        Barcode = barcode?.Trim();
        Size = size?.Trim();
        Color = color?.Trim();
        Dimensions = dimensions;
        CustomsCode = customsCode?.Trim();
    }

    internal void Deactivate()
    {
        Status = ProductVariantStatus.Inactive;
    }

    internal void Discontinue()
    {
        Status = ProductVariantStatus.Discontinued;
    }
}
