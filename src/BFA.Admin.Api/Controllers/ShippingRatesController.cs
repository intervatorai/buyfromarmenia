using BFA.Admin.Application.Commands.ShippingRates;
using BFA.Admin.Application.Queries.ShippingRates;
using BFA.BuildingBlocks.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/shipping-rates")]
[Authorize]
public sealed class ShippingRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetBrackets(
        [FromQuery] string? country,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetShippingRateBracketsQuery(country),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShippingPricingSettingsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("settings")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] UpdateShippingPricingSettingsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _mediator.Send(
                new UpdateShippingPricingSettingsCommand(request.ErrorMarginPercent),
                cancellationToken);
            return Ok(updated);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> CreateBracket(
        [FromBody] UpsertShippingRateBracketRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _mediator.Send(
                new CreateShippingRateBracketCommand(
                    request.CountryIsoCode,
                    request.WeightFromKg,
                    request.WeightToKg,
                    request.Price,
                    request.Currency,
                    request.IsActive),
                cancellationToken);
            return CreatedAtAction(nameof(GetBrackets), new { id = created.Id }, created);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateBracket(
        Guid id,
        [FromBody] UpdateShippingRateBracketRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _mediator.Send(
                new UpdateShippingRateBracketCommand(
                    id,
                    request.WeightFromKg,
                    request.WeightToKg,
                    request.Price,
                    request.Currency,
                    request.IsActive),
                cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> DeleteBracket(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteShippingRateBracketCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpsertShippingRateBracketRequest(
    string CountryIsoCode,
    decimal WeightFromKg,
    decimal WeightToKg,
    decimal Price,
    string Currency = "USD",
    bool IsActive = true);

public record UpdateShippingRateBracketRequest(
    decimal WeightFromKg,
    decimal WeightToKg,
    decimal Price,
    string Currency,
    bool IsActive);

public record UpdateShippingPricingSettingsRequest(decimal ErrorMarginPercent);
