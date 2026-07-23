namespace BFA.Public.Application.Services.Payments;

public sealed record StripeCheckoutSessionRequest(
    Guid OrderId,
    Guid PaymentId,
    string OrderNumber,
    string CustomerEmail,
    decimal Amount,
    string Currency,
    IReadOnlyList<StripeCheckoutLineItem> LineItems);

public sealed record StripeCheckoutLineItem(
    string Name,
    decimal UnitAmount,
    int Quantity);

public sealed record StripeCheckoutSessionResult(
    string SessionId,
    string CheckoutUrl);

public interface IStripeCheckoutService
{
    bool IsEnabled { get; }

    Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies signature and returns the Stripe event JSON type + SessionId + OrderId metadata.
    /// </summary>
    StripeWebhookEvent ParseWebhookEvent(string payload, string stripeSignatureHeader);
}

public sealed record StripeWebhookEvent(
    string EventId,
    string EventType,
    string? SessionId,
    Guid? OrderId,
    Guid? PaymentId,
    string? PaymentIntentId);
