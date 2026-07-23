using BFA.Admin.Application.Commands.ShippingCountries;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.ShippingCountries;

public record GetShippingCountriesQuery() : IRequest<IReadOnlyList<ShippingCountryDto>>;

public sealed class GetShippingCountriesQueryHandler
    : IRequestHandler<GetShippingCountriesQuery, IReadOnlyList<ShippingCountryDto>>
{
    private readonly IShippingCountryRepository _countryRepository;

    public GetShippingCountriesQueryHandler(IShippingCountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task<IReadOnlyList<ShippingCountryDto>> Handle(
        GetShippingCountriesQuery request,
        CancellationToken cancellationToken)
    {
        var countries = await _countryRepository.GetAllAsync(cancellationToken);
        return countries.Select(ShippingCountryMapper.ToDto).ToList();
    }
}
