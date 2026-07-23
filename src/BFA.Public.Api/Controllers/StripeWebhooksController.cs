using System.Text;
using BFA.Public.Application.Commands.Orders;
using BFA.Public.Application.Services.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public sealed class StripeWebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStripeCheckoutService _stripeCheckoutService;
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<StripeWebhooksController> _logger;

    public StripeWebhooksController(
        IMediator mediator,
        IStripeCheckoutService stripeCheckoutService,
        IOptions<StripeOptions> stripeOptions,
        ILogger<StripeWebhooksController> logger)
    {
        _mediator = mediator;
        _stripeCheckoutService = stripeCheckoutService;
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        if (!_stripeCheckoutService.IsEnabled
            || string.IsNullOrWhiteSpace(_stripeOptions.WebhookSecret)
            || _stripeOptions.WebhookSecret.Contains("YOUR_", StringComparison.Ordinal))
        {
            _logger.LogWarning("Stripe webhook received but Stripe is not fully configured.");
            return Ok(new { received = true, note = "Stripe not configured — event ignored." });
        }

        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            return BadRequest(new { code = "missing_signature", message = "Stripe-Signature header is required." });
        }

        StripeWebhookEvent parsed;
        try
        {
            parsed = _stripeCheckoutService.ParseWebhookEvent(payload, signatureHeader);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            return BadRequest(new { code = "invalid_signature", message = ex.Message });
        }

        _logger.LogInformation(
            "Stripe webhook {EventId} type {EventType} session {SessionId} order {OrderId}",
            parsed.EventId,
            parsed.EventType,
            parsed.SessionId,
            parsed.OrderId);

        try
        {
            var handled = await _mediator.Send(
                new CompleteStripeCheckoutCommand(
                    parsed.EventId,
                    parsed.EventType,
                    parsed.SessionId,
                    parsed.OrderId,
                    parsed.PaymentId,
                    parsed.PaymentIntentId),
                cancellationToken);

            return Ok(new
            {
                received = true,
                handled,
                eventId = parsed.EventId,
                eventType = parsed.EventType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Stripe webhook {EventId}.", parsed.EventId);
            return StatusCode(500, new { code = "processing_failed", message = "Webhook processing failed." });
        }
    }
}
