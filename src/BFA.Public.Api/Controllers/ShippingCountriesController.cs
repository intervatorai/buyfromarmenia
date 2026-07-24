using BFA.Public.Application.Queries.ShippingCountries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Route("api/shipping-countries")]
[AllowAnonymous]
public sealed class ShippingCountriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingCountriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetEnabledCountries(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEnabledShippingCountriesQuery(), cancellationToken);
        return Ok(result);
    }
}
