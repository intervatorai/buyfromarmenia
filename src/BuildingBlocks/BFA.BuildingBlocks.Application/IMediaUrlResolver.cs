namespace BFA.BuildingBlocks.Application;

public interface IMediaUrlResolver
{
    /// <summary>
    /// Builds a public URL from a storage key using configured CDN/base URL secrets.
    /// Absolute http(s) keys are returned as-is (legacy migration).
    /// </summary>
    string Resolve(string storageKey);
}
