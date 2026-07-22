using BFA.Supplier.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

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
    public async Task<IActionResult> GetDashboard(
        [FromQuery] Guid supplierId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSupplierDashboardQuery(supplierId),
            cancellationToken);
        return Ok(result);
    }
}
