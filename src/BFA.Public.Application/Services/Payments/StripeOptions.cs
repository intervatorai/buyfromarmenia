namespace BFA.Public.Application.Services.Payments;

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    /// <summary>
    /// When true and SecretKey is set, checkout uses Stripe Checkout Sessions.
    /// Otherwise payment is stub-captured immediately (local MVP fallback).
    /// </summary>
    public bool Enabled { get; set; }

    public string? SecretKey { get; set; }
    public string? PublishableKey { get; set; }
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Use {ORDER_ID} placeholder. Example: http://localhost:3200/orders/{ORDER_ID}?checkout=success
    /// </summary>
    public string SuccessUrl { get; set; } =
        "http://localhost:3200/orders/{ORDER_ID}?checkout=success";

    public string CancelUrl { get; set; } =
        "http://localhost:3200/checkout?checkout=cancelled";

    public bool IsConfigured =>
        Enabled
        && !string.IsNullOrWhiteSpace(SecretKey)
        && !SecretKey.Contains("YOUR_", StringComparison.Ordinal);
}
