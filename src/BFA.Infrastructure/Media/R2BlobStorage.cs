using Amazon.S3;
using Amazon.S3.Model;
using BFA.BuildingBlocks.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BFA.Infrastructure.Media;

/// <summary>Cloudflare R2 via S3-compatible API (same approach as Zentbow).</summary>
public sealed class R2BlobStorage : IBlobStorage, IDisposable
{
    private readonly IAmazonS3? _client;
    private readonly string? _bucketName;
    private readonly ILogger<R2BlobStorage> _logger;

    public R2BlobStorage(IConfiguration configuration, ILogger<R2BlobStorage> logger)
    {
        _logger = logger;
        var accountId = configuration["CloudflareR2:AccountId"];
        var accessKey = configuration["CloudflareR2:AccessKeyId"];
        var secretKey = configuration["CloudflareR2:SecretAccessKey"];
        _bucketName = configuration["CloudflareR2:BucketName"];

        if (string.IsNullOrWhiteSpace(accountId)
            || string.IsNullOrWhiteSpace(accessKey)
            || string.IsNullOrWhiteSpace(secretKey)
            || string.IsNullOrWhiteSpace(_bucketName))
        {
            logger.LogWarning("CloudflareR2 is not fully configured — product image uploads disabled.");
            _client = null;
            return;
        }

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId.Trim()}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
            AuthenticationRegion = "auto",
        };

        _client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public bool IsConfigured => _client is not null && !string.IsNullOrWhiteSpace(_bucketName);

    public async Task<string> UploadAsync(
        Stream content,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("R2 storage is not configured.");
        }

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true,
        };

        await _client!.PutObjectAsync(request, cancellationToken);
        _logger.LogInformation("Uploaded object to R2: {Key}", objectKey);
        return objectKey;
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return;
        }

        await _client!.DeleteObjectAsync(_bucketName, objectKey, cancellationToken);
        _logger.LogInformation("Deleted object from R2: {Key}", objectKey);
    }

    public void Dispose() => _client?.Dispose();
}
