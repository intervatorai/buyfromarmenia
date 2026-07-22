using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Media;

public record UploadProductImageCommand(
    Guid SupplierId,
    Stream Content,
    string FileName,
    string ContentType,
    long? ContentLength = null,
    Guid? ProductId = null,
    bool IsPrimary = true,
    string? AltText = null) : IRequest<UploadProductImageResult>;

public record UploadProductImageResult(
    Guid? MediaAssetId,
    string StorageKey,
    string Url,
    Guid? ProductMediaId);

public sealed class UploadProductImageCommandHandler
    : IRequestHandler<UploadProductImageCommand, UploadProductImageResult>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/gif",
    };

    private const long MaxBytes = 10 * 1024 * 1024;

    private readonly IBlobStorage _blobStorage;
    private readonly IMediaUrlResolver _mediaUrlResolver;
    private readonly IProductRepository _productRepository;

    public UploadProductImageCommandHandler(
        IBlobStorage blobStorage,
        IMediaUrlResolver mediaUrlResolver,
        IProductRepository productRepository)
    {
        _blobStorage = blobStorage;
        _mediaUrlResolver = mediaUrlResolver;
        _productRepository = productRepository;
    }

    public async Task<UploadProductImageResult> Handle(
        UploadProductImageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_blobStorage.IsConfigured)
        {
            throw new InvalidOperationException("Image storage is not configured.");
        }

        var contentType = string.IsNullOrWhiteSpace(request.ContentType)
            ? "application/octet-stream"
            : request.ContentType.Trim();

        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new InvalidOperationException("Only JPEG, PNG, WebP and GIF images are allowed.");
        }

        if (request.ContentLength is > MaxBytes)
        {
            throw new InvalidOperationException("Image must be 10 MB or smaller.");
        }

        if (request.ProductId.HasValue)
        {
            var owned = await _productRepository.GetByIdAsync(request.ProductId.Value, cancellationToken);
            if (owned is null || owned.SupplierId != request.SupplierId)
            {
                throw new InvalidOperationException("Product was not found.");
            }
        }

        var extension = ResolveExtension(request.FileName, contentType);
        var storageKey = BuildStorageKey(request.ProductId, extension);

        await using var buffer = new MemoryStream();
        await request.Content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length > MaxBytes)
        {
            throw new InvalidOperationException("Image must be 10 MB or smaller.");
        }

        buffer.Position = 0;
        await _blobStorage.UploadAsync(buffer, storageKey, contentType, cancellationToken);

        Guid? mediaAssetId = null;
        Guid? productMediaId = null;

        if (request.ProductId.HasValue)
        {
            var product = await _productRepository.GetByIdForUpdateAsync(
                request.ProductId.Value,
                cancellationToken)
                ?? throw new InvalidOperationException("Product was not found.");

            if (product.SupplierId != request.SupplierId)
            {
                throw new InvalidOperationException("Product was not found.");
            }

            var asset = MediaAsset.Create(storageKey, contentType, buffer.Length);
            var link = product.AddMedia(asset, request.AltText, isPrimary: request.IsPrimary);
            await _productRepository.UpdateAsync(product, cancellationToken);
            mediaAssetId = asset.Id;
            productMediaId = link.Id;
        }

        return new UploadProductImageResult(
            mediaAssetId,
            storageKey,
            _mediaUrlResolver.Resolve(storageKey),
            productMediaId);
    }

    private static string BuildStorageKey(Guid? productId, string extension)
    {
        var now = DateTime.UtcNow;
        var folder = productId.HasValue
            ? $"products/{productId:N}"
            : $"products/tmp/{now:yyyy}/{now:MM}";
        return $"{folder}/{Guid.NewGuid():N}{extension}";
    }

    private static string ResolveExtension(string fileName, string contentType)
    {
        var fromName = Path.GetExtension(fileName);
        if (!string.IsNullOrWhiteSpace(fromName) && fromName.Length <= 8)
        {
            return fromName.ToLowerInvariant();
        }

        return contentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => ".jpg",
        };
    }
}
