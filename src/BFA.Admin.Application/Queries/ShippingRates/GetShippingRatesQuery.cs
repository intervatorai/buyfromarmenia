using BFA.Admin.Application.Commands.ShippingRates;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.ShippingRates;

public record GetShippingRateBracketsQuery(string? CountryIsoCode = null)
    : IRequest<IReadOnlyList<ShippingRateBracketDto>>;

public record GetShippingPricingSettingsQuery : IRequest<ShippingPricingSettingsDto>;

public sealed class GetShippingRateBracketsQueryHandler
    : IRequestHandler<GetShippingRateBracketsQuery, IReadOnlyList<ShippingRateBracketDto>>
{
    private readonly IShippingRateBracketRepository _bracketRepository;

    public GetShippingRateBracketsQueryHandler(IShippingRateBracketRepository bracketRepository)
    {
        _bracketRepository = bracketRepository;
    }

    public async Task<IReadOnlyList<ShippingRateBracketDto>> Handle(
        GetShippingRateBracketsQuery request,
        CancellationToken cancellationToken)
    {
        var brackets = string.IsNullOrWhiteSpace(request.CountryIsoCode)
            ? await _bracketRepository.GetAllAsync(cancellationToken)
            : await _bracketRepository.GetByCountryAsync(request.CountryIsoCode, cancellationToken);

        return brackets.Select(ShippingRateMapper.ToDto).ToList();
    }
}

public sealed class GetShippingPricingSettingsQueryHandler
    : IRequestHandler<GetShippingPricingSettingsQuery, ShippingPricingSettingsDto>
{
    private readonly IShippingPricingSettingsRepository _settingsRepository;

    public GetShippingPricingSettingsQueryHandler(
        IShippingPricingSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<ShippingPricingSettingsDto> Handle(
        GetShippingPricingSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetOrCreateAsync(cancellationToken);
        return ShippingRateMapper.ToDto(settings);
    }
}
