using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Services;
using BFA.Modules.Shopping.Domain.Aggregates;

namespace BFA.Public.Application.Services.Shipping;

public static class CartShippingWeightEstimator
{
    public static decimal EstimateWeightKg(
        ShoppingCart cart,
        IReadOnlyDictionary<Guid, Product> productsById)
    {
        decimal total = 0m;

        foreach (var item in cart.Items)
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
            {
                throw new DomainException($"Product '{item.ProductId}' was not found for shipping quote.");
            }

            var variant = product.Variants.FirstOrDefault(v => v.Id == item.ProductVariantId)
                ?? throw new DomainException($"Variant '{item.ProductVariantId}' was not found for shipping quote.");

            var unitWeight = product.ShippingProfile?.GrossWeight ?? variant.Weight;
            if (unitWeight <= 0)
            {
                throw new DomainException(
                    $"Product '{product.Slug}' is missing shipping weight. Shipping cannot be calculated.");
            }

            total += unitWeight * item.Quantity;
        }

        return Math.Round(total, 3, MidpointRounding.AwayFromZero);
    }
}

public interface IShippingQuoteService
{
    Task<ShippingQuoteResult> QuoteAsync(
        string countryIsoCode,
        decimal estimatedWeightKg,
        CancellationToken cancellationToken = default);
}

public sealed class ShippingQuoteService : IShippingQuoteService
{
    private readonly IShippingRateBracketRepository _bracketRepository;
    private readonly IShippingPricingSettingsRepository _settingsRepository;
    private readonly IShippingCountryRepository _countryRepository;

    public ShippingQuoteService(
        IShippingRateBracketRepository bracketRepository,
        IShippingPricingSettingsRepository settingsRepository,
        IShippingCountryRepository countryRepository)
    {
        _bracketRepository = bracketRepository;
        _settingsRepository = settingsRepository;
        _countryRepository = countryRepository;
    }

    public async Task<ShippingQuoteResult> QuoteAsync(
        string countryIsoCode,
        decimal estimatedWeightKg,
        CancellationToken cancellationToken = default)
    {
        if (estimatedWeightKg <= 0)
        {
            throw new DomainException("Estimated weight must be positive.");
        }

        var country = await _countryRepository.GetByIsoCodeAsync(countryIsoCode, cancellationToken);
        if (country is null || !country.IsEnabled)
        {
            throw new DomainException($"Shipping to '{countryIsoCode}' is not available.");
        }

        var bracket = await _bracketRepository.GetActiveForWeightAsync(
            country.IsoCode,
            estimatedWeightKg,
            cancellationToken);
        if (bracket is null)
        {
            throw new DomainException(
                $"No shipping rate is configured for {country.IsoCode} at {estimatedWeightKg:0.###} kg.");
        }

        var settings = await _settingsRepository.GetOrCreateAsync(cancellationToken);

        return ShippingQuoteCalculator.Calculate(
            country.IsoCode,
            estimatedWeightKg,
            bracket.Price,
            settings.ErrorMarginPercent,
            bracket.Currency,
            bracket.Id);
    }
}
