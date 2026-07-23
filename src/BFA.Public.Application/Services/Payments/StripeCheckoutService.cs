using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace BFA.Public.Application.Services.Payments;

public sealed class StripeCheckoutService : IStripeCheckoutService
{
    private readonly StripeOptions _options;
    private readonly ILogger<StripeCheckoutService> _logger;

    public StripeCheckoutService(
        IOptions<StripeOptions> options,
        ILogger<StripeCheckoutService> logger)
    {
        _options = options.Value;
        _logger = logger;
        if (_options.IsConfigured)
        {
            StripeConfiguration.ApiKey = _options.SecretKey;
        }
    }

    public bool IsEnabled => _options.IsConfigured;

    public async Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Stripe is not configured.");
        }

        var successUrl = _options.SuccessUrl.Replace(
            "{ORDER_ID}",
            request.OrderId.ToString("D"),
            StringComparison.OrdinalIgnoreCase);
        var cancelUrl = _options.CancelUrl.Replace(
            "{ORDER_ID}",
            request.OrderId.ToString("D"),
            StringComparison.OrdinalIgnoreCase);

        var currency = request.Currency.Trim().ToLowerInvariant();
        var lineItems = request.LineItems.Select(item => new SessionLineItemOptions
        {
            Quantity = item.Quantity,
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = currency,
                UnitAmount = ToStripeAmount(item.UnitAmount, currency),
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.Name
                }
            }
        }).ToList();

        // Include shipping as its own line when the order total may exceed item sum —
        // PlaceOrder passes explicit line items including a shipping row when needed.
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            CustomerEmail = request.CustomerEmail,
            ClientReferenceId = request.OrderId.ToString("D"),
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems = lineItems,
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = request.OrderId.ToString("D"),
                ["paymentId"] = request.PaymentId.ToString("D"),
                ["orderNumber"] = request.OrderNumber
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = request.OrderId.ToString("D"),
                    ["paymentId"] = request.PaymentId.ToString("D")
                }
            },
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(session.Url))
        {
            throw new InvalidOperationException("Stripe Checkout Session did not return a URL.");
        }

        _logger.LogInformation(
            "Created Stripe Checkout Session {SessionId} for order {OrderId}.",
            session.Id,
            request.OrderId);

        return new StripeCheckoutSessionResult(session.Id, session.Url);
    }

    public StripeWebhookEvent ParseWebhookEvent(string payload, string stripeSignatureHeader)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            throw new InvalidOperationException("Stripe webhook secret is not configured.");
        }

        var stripeEvent = EventUtility.ConstructEvent(
            payload,
            stripeSignatureHeader,
            _options.WebhookSecret,
            throwOnApiVersionMismatch: false);

        string? sessionId = null;
        Guid? orderId = null;
        Guid? paymentId = null;
        string? paymentIntentId = null;

        if (stripeEvent.Data.Object is Session session)
        {
            sessionId = session.Id;
            paymentIntentId = session.PaymentIntentId;
            if (session.Metadata is not null)
            {
                if (session.Metadata.TryGetValue("orderId", out var orderRaw)
                    && Guid.TryParse(orderRaw, out var parsedOrder))
                {
                    orderId = parsedOrder;
                }

                if (session.Metadata.TryGetValue("paymentId", out var paymentRaw)
                    && Guid.TryParse(paymentRaw, out var parsedPayment))
                {
                    paymentId = parsedPayment;
                }
            }

            if (!orderId.HasValue
                && Guid.TryParse(session.ClientReferenceId, out var fromClientRef))
            {
                orderId = fromClientRef;
            }
        }

        return new StripeWebhookEvent(
            stripeEvent.Id,
            stripeEvent.Type,
            sessionId,
            orderId,
            paymentId,
            paymentIntentId);
    }

    private static long ToStripeAmount(decimal amount, string currency)
    {
        // Most currencies use 2 decimal places. Zero-decimal currencies are rare for this MVP.
        _ = currency;
        return (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
    }
}
