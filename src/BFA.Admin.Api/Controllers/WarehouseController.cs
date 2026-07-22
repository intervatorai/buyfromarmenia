using BFA.Admin.Application.Commands.Warehouse;
using BFA.Admin.Application.Queries.Warehouse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class WarehouseController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehouseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("inbound")]
    public async Task<IActionResult> GetInboundShipments(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetInboundShipmentsQuery(status),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("inbound/{id:guid}/arrived")]
    public async Task<IActionResult> MarkArrived(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new MarkInboundShipmentArrivedCommand(id),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("inbound/{id:guid}/receive")]
    public async Task<IActionResult> Receive(
        Guid id,
        [FromBody] ReceiveInboundShipmentRequest request,
        CancellationToken cancellationToken)
    {
        var receivedBy = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? "warehouse-operator";

        var updated = await _mediator.Send(
            new ReceiveInboundShipmentCommand(
                id,
                request.ScanReference,
                request.WeightKg,
                request.InspectionNotes,
                request.PhotoUrl,
                receivedBy),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpGet("inbound/eligible")]
    public async Task<IActionResult> GetEligibleInboundShipments(
        [FromQuery] Guid? customerOrderId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetEligibleInboundShipmentsQuery(customerOrderId),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("consolidations")]
    public async Task<IActionResult> GetConsolidations(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetConsolidationsQuery(status),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("consolidations")]
    public async Task<IActionResult> CreateConsolidation(
        [FromBody] CreateConsolidationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateConsolidationCommand(
                request.CustomerOrderId,
                request.InboundShipmentIds),
            cancellationToken);

        return result is null
            ? BadRequest(new { message = "Unable to create consolidation." })
            : CreatedAtAction(nameof(GetConsolidations), new { }, result);
    }

    [HttpPost("consolidations/{id:guid}/packages")]
    public async Task<IActionResult> AddPackage(
        Guid id,
        [FromBody] AddPackageRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new AddPackageToConsolidationCommand(id, request.WeightKg, request.Notes),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("consolidations/{id:guid}/seal")]
    public async Task<IActionResult> SealConsolidation(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new SealConsolidationCommand(id), cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}

public record CreateConsolidationRequest(
    Guid CustomerOrderId,
    IReadOnlyList<Guid> InboundShipmentIds);

public record AddPackageRequest(decimal WeightKg, string? Notes);

public record ReceiveInboundShipmentRequest(
    string ScanReference,
    decimal WeightKg,
    string? InspectionNotes,
    string? PhotoUrl);
