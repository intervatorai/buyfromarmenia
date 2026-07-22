using BFA.Admin.Application.Queries.Audit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrAbove")]
public sealed class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditEntries(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var entries = await _mediator.Send(new GetAuditEntriesQuery(take), cancellationToken);
        return Ok(entries);
    }
}
