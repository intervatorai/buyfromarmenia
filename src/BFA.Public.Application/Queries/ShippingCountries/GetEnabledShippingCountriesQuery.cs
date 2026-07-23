using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.ShippingCountries;

public record PublicShippingCountryDto(
    string IsoCode,
    string NameEn,
    string NameHy,
    int SortOrder);

public record GetEnabledShippingCountriesQuery()
    : IRequest<IReadOnlyList<PublicShippingCountryDto>>;

public sealed class GetEnabledShippingCountriesQueryHandler
    : IRequestHandler<GetEnabledShippingCountriesQuery, IReadOnlyList<PublicShippingCountryDto>>
{
    private readonly IShippingCountryRepository _countryRepository;

    public GetEnabledShippingCountriesQueryHandler(IShippingCountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<IReadOnlyList<PublicShippingCountryDto>> Handle(
        GetEnabledShippingCountriesQuery request,
        CancellationToken cancellationToken)
    {
        var countries = await _countryRepository.GetEnabledAsync(cancellationToken);
        return countries
            .Select(country => new PublicShippingCountryDto(
                country.IsoCode,
                country.NameEn,
                country.NameHy,
                country.SortOrder))
            .ToList();
    }
}
