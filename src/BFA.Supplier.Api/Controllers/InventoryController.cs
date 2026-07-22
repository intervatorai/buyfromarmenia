using BFA.Supplier.Application.Commands.Inventory;
using BFA.Supplier.Application.Queries.Inventory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetStock(
        [FromQuery] Guid supplierId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSupplierStockQuery(supplierId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("variants/{variantId:guid}")]
    public async Task<IActionResult> SetStock(
        Guid variantId,
        [FromBody] SetStockRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new SetStockCommand(
                request.SupplierId,
                request.ProductId,
                variantId,
                request.OnHand,
                request.LowStockThreshold),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}

public record SetStockRequest(
    Guid SupplierId,
    Guid ProductId,
    int OnHand,
    int LowStockThreshold = 5);
