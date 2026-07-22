using BFA.Admin.Application.Commands.Compliance;
using BFA.Admin.Application.Queries.Compliance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/compliance/restrictions")]
[Authorize(Policy = "AdminOrAbove")]
public sealed class ComplianceController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplianceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRestrictions(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTradeRestrictionsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRestriction(
        [FromBody] CreateTradeRestrictionRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateTradeRestrictionCommand(
                request.DestinationCountryCode,
                request.Reason,
                request.CategoryId),
            cancellationToken);

        return CreatedAtAction(nameof(GetRestrictions), new { id }, new { id });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateRestriction(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deactivated = await _mediator.Send(
            new DeactivateTradeRestrictionCommand(id),
            cancellationToken);

        return deactivated ? NoContent() : NotFound();
    }
}

public record CreateTradeRestrictionRequest(
    string DestinationCountryCode,
    string Reason,
    Guid? CategoryId = null);
