using BFA.Admin.Application.Commands.ShippingCountries;
using BFA.Admin.Application.Queries.ShippingCountries;
using BFA.BuildingBlocks.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/shipping-countries")]
[Authorize]
public sealed class ShippingCountriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingCountriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShippingCountriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> CreateCountry(
        [FromBody] UpsertShippingCountryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _mediator.Send(
                new CreateShippingCountryCommand(
                    request.IsoCode,
                    request.NameEn,
                    request.NameHy,
                    request.SortOrder,
                    request.IsEnabled),
                cancellationToken);

            return CreatedAtAction(nameof(GetCountries), new { id = created.Id }, created);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateCountry(
        Guid id,
        [FromBody] UpdateShippingCountryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _mediator.Send(
                new UpdateShippingCountryCommand(
                    id,
                    request.NameEn,
                    request.NameHy,
                    request.SortOrder),
                cancellationToken);

            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("seed-defaults")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> SeedDefaults(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SeedDefaultShippingCountriesCommand(),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/enable")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> EnableCountry(Guid id, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new SetShippingCountryEnabledCommand(id, true),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/disable")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> DisableCountry(Guid id, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new SetShippingCountryEnabledCommand(id, false),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}

public record UpsertShippingCountryRequest(
    string IsoCode,
    string NameEn,
    string NameHy,
    int SortOrder = 0,
    bool IsEnabled = true);

public record UpdateShippingCountryRequest(
    string NameEn,
    string NameHy,
    int SortOrder);
