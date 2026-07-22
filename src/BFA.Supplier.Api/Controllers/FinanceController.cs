using BFA.Supplier.Application.Queries.Finance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetFinance(
        [FromQuery] Guid supplierId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSupplierFinanceQuery(supplierId),
            cancellationToken);
        return Ok(result);
    }
}
