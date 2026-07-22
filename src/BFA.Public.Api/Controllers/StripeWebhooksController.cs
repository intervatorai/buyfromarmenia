using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BFA.Public.Api.Controllers;

/// <summary>
/// Stripe webhook skeleton. Signature verification and payment capture wiring come in Phase 3.
/// </summary>
[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public sealed class StripeWebhooksController : ControllerBase
{
    private readonly ILogger<StripeWebhooksController> _logger;

    public StripeWebhooksController(ILogger<StripeWebhooksController> logger)
    {
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            _logger.LogWarning("Stripe webhook received without Stripe-Signature header.");
        }

        string? eventType = null;
        string? eventId = null;

        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(payload) ? "{}" : payload);
            if (document.RootElement.TryGetProperty("type", out var typeProperty))
            {
                eventType = typeProperty.GetString();
            }

            if (document.RootElement.TryGetProperty("id", out var idProperty))
            {
                eventId = idProperty.GetString();
            }
        }
        catch (JsonException)
        {
            return BadRequest(new { code = "invalid_payload", message = "Unable to parse Stripe event JSON." });
        }

        _logger.LogInformation(
            "Stripe webhook stub accepted event {EventId} of type {EventType}.",
            eventId ?? "unknown",
            eventType ?? "unknown");

        return Ok(new
        {
            received = true,
            eventId,
            eventType,
            note = "Stub only — payment capture not wired yet."
        });
    }
}
