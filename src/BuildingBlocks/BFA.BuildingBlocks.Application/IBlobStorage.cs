namespace BFA.BuildingBlocks.Application;

public interface IBlobStorage
{
    bool IsConfigured { get; }

    Task<string> UploadAsync(
        Stream content,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
}
