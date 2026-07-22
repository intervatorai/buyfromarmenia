using BFA.Admin.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery(), cancellationToken);
        return Ok(result);
    }
}
