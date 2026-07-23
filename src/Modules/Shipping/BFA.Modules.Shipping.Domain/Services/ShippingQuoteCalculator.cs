namespace BFA.Modules.Shipping.Domain.Services;

public sealed record ShippingQuoteResult(
    decimal EstimatedWeightKg,
    decimal BasePrice,
    decimal ErrorMarginPercent,
    decimal ShippingFee,
    string Currency,
    Guid BracketId,
    string CountryIsoCode);

public static class ShippingQuoteCalculator
{
    public static ShippingQuoteResult Calculate(
        string countryIsoCode,
        decimal estimatedWeightKg,
        decimal basePrice,
        decimal errorMarginPercent,
        string currency,
        Guid bracketId)
    {
        var fee = Math.Round(
            basePrice * (1m + errorMarginPercent / 100m),
            2,
            MidpointRounding.AwayFromZero);

        return new ShippingQuoteResult(
            estimatedWeightKg,
            basePrice,
            errorMarginPercent,
            fee,
            currency,
            bracketId,
            countryIsoCode);
    }
}
