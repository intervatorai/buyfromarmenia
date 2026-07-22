using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BFA.BuildingBlocks.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BFA.Infrastructure.Search;

public sealed class ProductSearchKeywordGenerator : IProductSearchKeywordGenerator
{
    private const int MaxKeywordsLength = 500;
    private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "are", "as", "at", "be", "by", "for", "from", "in", "is", "it",
        "of", "on", "or", "the", "to", "with", "this", "that", "your", "our", "product",
        "և", "ու", "ի", "ից", "ում", "համար", "հետ", "այս", "այն"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductSearchKeywordGenerator> _logger;
    private readonly bool _openAiEnabled;
    private readonly string? _apiKey;
    private readonly string _model;

    public ProductSearchKeywordGenerator(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ProductSearchKeywordGenerator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"];
        _openAiEnabled = !string.IsNullOrWhiteSpace(_apiKey)
            && configuration.GetValue("OpenAI:Enabled", false);
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
    }

    public async Task<string> GenerateAsync(
        ProductSearchKeywordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_openAiEnabled)
        {
            try
            {
                var aiKeywords = await GenerateWithOpenAiAsync(request, cancellationToken);
                if (!string.IsNullOrWhiteSpace(aiKeywords))
                {
                    return NormalizeKeywords(aiKeywords);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI keyword generation failed; falling back to rule-based keywords.");
            }
        }

        return NormalizeKeywords(GenerateRuleBased(request));
    }

    private static string GenerateRuleBased(ProductSearchKeywordRequest request)
    {
        var tokens = request.Translations
            .SelectMany(translation => new[]
            {
                translation.Name,
                translation.ShortDescription,
                translation.Description
            })
            .Append(request.CategoryName ?? string.Empty)
            .SelectMany(Tokenize)
            .Where(token => token.Length > 2 && !StopWords.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(30);

        return string.Join(", ", tokens);
    }

    private async Task<string?> GenerateWithOpenAiAsync(
        ProductSearchKeywordRequest request,
        CancellationToken cancellationToken)
    {
        var contentBuilder = new StringBuilder();
        foreach (var translation in request.Translations)
        {
            contentBuilder.AppendLine($"Language: {translation.LanguageCode}");
            contentBuilder.AppendLine($"Name: {translation.Name}");
            if (!string.IsNullOrWhiteSpace(translation.ShortDescription))
            {
                contentBuilder.AppendLine($"Short: {translation.ShortDescription}");
            }

            if (!string.IsNullOrWhiteSpace(translation.Description))
            {
                contentBuilder.AppendLine($"Description: {translation.Description}");
            }

            contentBuilder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryName))
        {
            contentBuilder.AppendLine($"Category: {request.CategoryName}");
        }

        var payload = new
        {
            model = _model,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
You extract search keywords for an Armenian products marketplace (BuyFromArmenia).
Return 15-25 lowercase comma-separated keywords only.
Include English and Armenian terms, synonyms, and related search phrases.
No explanations.
"""
                },
                new
                {
                    role = "user",
                    content = contentBuilder.ToString()
                }
            }
        };

        using var client = _httpClientFactory.CreateClient(nameof(ProductSearchKeywordGenerator));
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, OpenAiApiUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await client.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (Match match in Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{N}]{2,}"))
        {
            yield return match.Value;
        }
    }

    private static string NormalizeKeywords(string keywords)
    {
        var normalized = string.Join(
            ", ",
            keywords
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => part.ToLowerInvariant())
                .Where(part => part.Length > 1 && !StopWords.Contains(part))
                .Distinct(StringComparer.Ordinal)
                .Take(40));

        return normalized.Length <= MaxKeywordsLength
            ? normalized
            : normalized[..MaxKeywordsLength].TrimEnd(',', ' ');
    }
}
