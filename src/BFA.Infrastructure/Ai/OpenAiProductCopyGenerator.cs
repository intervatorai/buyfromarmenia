using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BFA.BuildingBlocks.Application;
using Microsoft.Extensions.Configuration;

namespace BFA.Infrastructure.Ai;

public sealed class OpenAiProductCopyGenerator : IProductCopyGenerator
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly bool _enabled;

    public OpenAiProductCopyGenerator(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = configuration["OpenAI:ApiKey"];
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        _enabled = configuration.GetValue("OpenAI:Enabled", false)
            && !string.IsNullOrWhiteSpace(_apiKey);
    }

    public bool IsEnabled => _enabled;

    public async Task<ProductCopyResult> GenerateAsync(
        ProductCopyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException(
                "AI copy generation is disabled. Configure OpenAI:Enabled and OpenAI:ApiKey.");
        }

        var language = request.LanguageCode.ToLowerInvariant() switch
        {
            "en" => "English",
            "hy" => "Armenian",
            _ => throw new InvalidOperationException("Only English and Armenian are supported.")
        };

        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            throw new InvalidOperationException("Enter the product name first.");
        }

        if (!ProductCopyField.IsValid(request.Field))
        {
            throw new InvalidOperationException(
                $"Unknown field '{request.Field}'. Use '{ProductCopyField.ShortDescription}' or '{ProductCopyField.Description}'.");
        }

        var isShort = request.Field == ProductCopyField.ShortDescription;
        var targetText = isShort ? request.ShortDescription : request.Description;
        var mode = string.IsNullOrWhiteSpace(targetText)
            ? "GENERATE new text for the target field"
            : "POLISH the current target field text";

        var userPrompt = $"""
Product name: {request.ProductName.Trim()}
Target language: {language}
Target field: {(isShort ? "SHORT DESCRIPTION" : "DESCRIPTION")}
Mode: {mode}

Current target field text:
---
{targetText?.Trim() ?? ""}
---

Context — the other field (do NOT rewrite it, use only as factual context):
---
{(isShort ? request.Description : request.ShortDescription)?.Trim() ?? ""}
---
""";

        var fieldRules = isShort
            ? "The target is the SHORT DESCRIPTION: one concise, appealing sentence, maximum 160 characters."
            : "The target is the DESCRIPTION: 2-4 natural paragraphs, approximately 80-140 words.";

        var payload = new
        {
            model = _model,
            temperature = 0.45,
            max_tokens = isShort ? 300 : 1200,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = $$"""
You write high-quality product copy for BuyFromArmenia, a marketplace for Armenian products.
Write ONLY in {{language}}. Never translate into another language.

You work on exactly ONE target field per request.
If the current target field text is blank, generate it from the product name and context.
If it exists, polish grammar, clarity, tone, and readability while preserving its meaning.
The product name, current text, and provided context are the ONLY factual sources.
Do not infer or invent ingredients, materials, manufacturing methods, geography, origin,
quality level, health benefits, nutrition, certifications, guarantees, dimensions, or prices.
In particular, never claim “natural”, “traditional”, “finest”, “sun-ripened”, “healthy”,
“rich in vitamins”, or similar unless those exact facts are present in the current text.
When facts are limited, write useful neutral copy about what the named product is and
ordinary ways a customer may use, wear, display, serve, or gift that product.

{{fieldRules}}

Return STRICT JSON only:
{"text":"..."}

Rules:
- Plain text only; no HTML or Markdown.
- Avoid generic filler and exaggerated claims.
"""
                },
                new { role = "user", content = userPrompt }
            }
        };

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var client = _httpClientFactory.CreateClient(nameof(OpenAiProductCopyGenerator));
        using var response = await client.SendAsync(requestMessage, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ReadOpenAiError(raw, response.StatusCode));
        }

        using var document = JsonDocument.Parse(raw);
        var content = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("OpenAI returned an empty response.");
        }

        using var resultDocument = JsonDocument.Parse(content);
        var text = resultDocument.RootElement.GetProperty("text").GetString()?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("OpenAI response is missing product copy.");
        }

        return new ProductCopyResult(text);
    }

    private static string ReadOpenAiError(string raw, System.Net.HttpStatusCode statusCode)
    {
        try
        {
            using var document = JsonDocument.Parse(raw);
            var message = document.RootElement
                .GetProperty("error")
                .GetProperty("message")
                .GetString();
            if (!string.IsNullOrWhiteSpace(message))
            {
                return message;
            }
        }
        catch (JsonException)
        {
        }

        return $"OpenAI request failed with HTTP {(int)statusCode}.";
    }
}
