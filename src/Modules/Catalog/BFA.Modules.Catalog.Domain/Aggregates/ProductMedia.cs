using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Catalog.Domain.Aggregates;

/// <summary>
/// Join between <see cref="Product"/> and <see cref="MediaAsset"/> (one product → many images).
/// </summary>
public sealed class ProductMedia : Entity
{
    public Guid ProductId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public MediaAsset MediaAsset { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    private ProductMedia()
    {
    }

    internal ProductMedia(
        Guid productId,
        MediaAsset mediaAsset,
        string? altText = null,
        int sortOrder = 0,
        bool isPrimary = false)
    {
        ArgumentNullException.ThrowIfNull(mediaAsset);

        Id = Guid.NewGuid();
        ProductId = productId;
        MediaAsset = mediaAsset;
        MediaAssetId = mediaAsset.Id;
        AltText = altText?.Trim();
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
    }

    internal void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }

    internal void Update(string? altText, int sortOrder, bool isPrimary)
    {
        AltText = altText?.Trim();
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
    }
}
