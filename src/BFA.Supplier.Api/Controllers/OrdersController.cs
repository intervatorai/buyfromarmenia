using BFA.Supplier.Application.Commands.Orders;
using BFA.Supplier.Application.Queries.Orders;using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] Guid supplierId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSupplierOrdersQuery(supplierId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateSupplierOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new UpdateSupplierOrderStatusCommand(
                request.SupplierId,
                id,
                request.Status),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/transfer")]
    public async Task<IActionResult> TransferToWarehouse(
        Guid id,
        [FromBody] TransferSupplierOrderRequest request,
        CancellationToken cancellationToken)
    {
        var transferred = await _mediator.Send(
            new TransferSupplierOrderToWarehouseCommand(request.SupplierId, id),
            cancellationToken);

        return transferred ? NoContent() : NotFound();
    }
}

public record TransferSupplierOrderRequest(Guid SupplierId);

public record UpdateSupplierOrderStatusRequest(Guid SupplierId, string Status);
