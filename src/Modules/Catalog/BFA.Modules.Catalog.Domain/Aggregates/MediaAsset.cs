using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Catalog.Domain.Aggregates;

/// <summary>
/// Stored object in blob storage. Only the storage key is persisted — public URLs are resolved from configuration.
/// </summary>
public sealed class MediaAsset : Entity
{
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long? ByteSize { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MediaAsset()
    {
    }

    private MediaAsset(string storageKey, string contentType, long? byteSize)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new DomainException("Storage key is required.");
        }

        Id = Guid.NewGuid();
        StorageKey = NormalizeKey(storageKey);
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim().ToLowerInvariant();
        ByteSize = byteSize is > 0 ? byteSize : null;
        CreatedAt = DateTime.UtcNow;
    }

    public static MediaAsset Create(string storageKey, string contentType = "image/jpeg", long? byteSize = null)
        => new(storageKey, contentType, byteSize);

    private static string NormalizeKey(string storageKey)
    {
        var key = storageKey.Trim().Replace('\\', '/');
        while (key.StartsWith('/'))
        {
            key = key[1..];
        }

        return key;
    }
}
