using BFA.Admin.Application.Commands.Shipping;
using BFA.Admin.Application.Queries.Shipping;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class LogisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("shipments")]
    public async Task<IActionResult> GetShipments(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShipmentsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("shipments")]
    public async Task<IActionResult> CreateShipment(
        [FromBody] CreateShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateInternationalShipmentCommand(
                request.ConsolidationId,
                request.Carrier,
                request.CustomsDescription),
            cancellationToken);

        return result is null
            ? BadRequest(new { message = "Unable to create shipment." })
            : Ok(result);
    }

    [HttpPost("shipments/{id:guid}/advance")]
    public async Task<IActionResult> AdvanceShipment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new AdvanceShipmentStatusCommand(id), cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}

public record CreateShipmentRequest(
    Guid ConsolidationId,
    string Carrier,
    string CustomsDescription);
