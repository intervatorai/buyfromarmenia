using BFA.Admin.Application.Commands.Orders;
using BFA.Admin.Application.Queries.Orders;
using BFA.BuildingBlocks.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerOrdersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerOrderQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/adjust-shipping")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> AdjustShipping(
        Guid id,
        [FromBody] AdjustOrderShippingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(
                new AdjustOrderShippingCommand(
                    id,
                    request.ActualWeightKg,
                    request.ManualShippingFee,
                    request.Reason),
                cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> SetStatus(
        Guid id,
        [FromBody] SetOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(
                new SetAdminOrderStatusCommand(
                    id,
                    request.OrderStatus,
                    request.PaymentStatus),
                cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record AdjustOrderShippingRequest(
    decimal? ActualWeightKg,
    decimal? ManualShippingFee,
    string? Reason);

public record SetOrderStatusRequest(
    BFA.Modules.Ordering.Domain.Enums.OrderStatus? OrderStatus,
    BFA.Modules.Ordering.Domain.Enums.PaymentStatus? PaymentStatus);
