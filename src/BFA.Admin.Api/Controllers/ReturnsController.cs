using BFA.Admin.Application.Commands.Returns;
using BFA.Admin.Application.Queries.Returns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ModeratorOrAbove")]
public sealed class ReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetReturns(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReturnRequestsQuery(status), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ReturnModerationRequest? request,
        CancellationToken cancellationToken)
    {
        var approved = await _mediator.Send(
            new ApproveReturnRequestCommand(id, request?.Notes),
            cancellationToken);

        return approved ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] ReturnModerationRequest request,
        CancellationToken cancellationToken)
    {
        var rejected = await _mediator.Send(
            new RejectReturnRequestCommand(id, request.Notes ?? "Rejected"),
            cancellationToken);

        return rejected ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> MarkReceived(
        Guid id,
        [FromBody] ReturnModerationRequest? request,
        CancellationToken cancellationToken)
    {
        var received = await _mediator.Send(
            new MarkReturnReceivedCommand(id, request?.Notes),
            cancellationToken);
        return received ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/refund")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Refund(Guid id, CancellationToken cancellationToken)
    {
        var refunded = await _mediator.Send(new RefundReturnRequestCommand(id), cancellationToken);
        return refunded ? NoContent() : NotFound();
    }
}

public record ReturnModerationRequest(string? Notes);
