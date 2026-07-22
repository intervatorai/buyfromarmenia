using BFA.BuildingBlocks.Application;
using Microsoft.Extensions.Configuration;

namespace BFA.Infrastructure.Media;

public sealed class MediaUrlResolver : IMediaUrlResolver
{
    private readonly string _publicBaseUrl;

    public MediaUrlResolver(IConfiguration configuration)
    {
        _publicBaseUrl = (
            configuration["Media:PublicBaseUrl"]
            ?? configuration["CloudflareR2:PublicUrl"]
            ?? string.Empty
        ).Trim().TrimEnd('/');
    }

    public string Resolve(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return string.Empty;
        }

        var key = storageKey.Trim();
        if (key.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return key;
        }

        key = key.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(_publicBaseUrl))
        {
            return key;
        }

        return $"{_publicBaseUrl}/{key}";
    }
}
