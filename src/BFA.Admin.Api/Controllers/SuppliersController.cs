using BFA.Admin.Application.Commands.Suppliers;
using BFA.Admin.Application.Queries.Suppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var suppliers = await _mediator.Send(new GetSuppliersQuery(status), cancellationToken);
        return Ok(suppliers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _mediator.Send(new GetSupplierQuery(id), cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> CreateSupplier(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateSupplierCommand(
                request.LegalName,
                request.TradingName,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.TaxNumber,
                request.RegistrationNumber,
                request.ActivateImmediately),
            cancellationToken);

        return CreatedAtAction(nameof(GetSupplier), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new UpdateSupplierCommand(
                id,
                request.LegalName,
                request.TradingName,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.TaxNumber,
                request.RegistrationNumber),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var approved = await _mediator.Send(new ApproveSupplierCommand(id), cancellationToken);
        return approved ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var rejected = await _mediator.Send(
            new RejectSupplierCommand(id, request.Reason),
            cancellationToken);

        return rejected ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/request-changes")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> RequestChanges(
        Guid id,
        [FromBody] RejectSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new RequestSupplierChangesCommand(id, request.Reason),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Suspend(
        Guid id,
        [FromBody] RejectSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var suspended = await _mediator.Send(
            new SuspendSupplierCommand(id, request.Reason),
            cancellationToken);
        return suspended ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var activated = await _mediator.Send(new ActivateSupplierCommand(id), cancellationToken);
        return activated ? NoContent() : NotFound();
    }
}

public record RejectSupplierRequest(string Reason);

public record CreateSupplierRequest(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber = null,
    string? RegistrationNumber = null,
    bool ActivateImmediately = false);

public record UpdateSupplierRequest(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber = null,
    string? RegistrationNumber = null);
