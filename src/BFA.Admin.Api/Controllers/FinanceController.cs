using BFA.Admin.Application.Commands.Finance;
using BFA.Admin.Application.Queries.Finance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrAbove")]
public sealed class FinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminFinanceSummaryQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("settlements")]
    public async Task<IActionResult> GetSettlements(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSettlementsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("payouts")]
    public async Task<IActionResult> GetPayouts(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPayoutsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("settlements/{id:guid}/eligible")]
    public async Task<IActionResult> MarkEligible(Guid id, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new MarkSettlementEligibleCommand(id), cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("settlements/{id:guid}/payout")]
    public async Task<IActionResult> CreatePayout(Guid id, CancellationToken cancellationToken)
    {
        var payoutId = await _mediator.Send(new CreatePayoutFromSettlementCommand(id), cancellationToken);
        return payoutId is null ? NotFound() : Ok(new { id = payoutId });
    }

    [HttpPost("payouts/{id:guid}/complete")]
    public async Task<IActionResult> CompletePayout(Guid id, CancellationToken cancellationToken)
    {
        var completed = await _mediator.Send(new CompletePayoutCommand(id), cancellationToken);
        return completed ? NoContent() : NotFound();
    }
}
