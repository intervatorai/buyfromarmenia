using System.Security.Claims;
using BFA.BuildingBlocks.Domain;
using BFA.Public.Application.Commands.Orders;
using BFA.Public.Application.Queries.Orders;
using BFA.Public.Application.Queries.Shipping;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

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
    [Authorize]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var customerUserId = GetCustomerUserId();
        if (!customerUserId.HasValue)
        {
            return Unauthorized();
        }

        var customerOrders = await _mediator.Send(
            new GetOrdersByCustomerQuery(customerUserId.Value),
            cancellationToken);
        return Ok(customerOrders);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var customerUserId = GetCustomerUserId();
        if (!customerUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(
            new GetOrderQuery(id, customerUserId.Value),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/shipment")]
    [Authorize]
    public async Task<IActionResult> GetOrderShipment(Guid id, CancellationToken cancellationToken)
    {
        var customerUserId = GetCustomerUserId();
        if (!customerUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(
            new GetOrderShipmentQuery(id, customerUserId.Value),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customerUserId = GetCustomerUserId();
        if (!customerUserId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(
            new PlaceOrderCommand(
                request.CartId,
                request.CustomerEmail,
                request.CustomerFullName,
                request.DeliveryAddressId,
                customerUserId.Value),
            cancellationToken);

        if (result is PlaceOrderSuccess success)
        {
            return CreatedAtAction(
                nameof(GetOrder),
                new { id = success.OrderId },
                new
                {
                    orderId = success.OrderId,
                    orderNumber = success.OrderNumber,
                    checkoutUrl = success.CheckoutUrl
                });
        }

        if (result is PlaceOrderComplianceBlocked blocked)
        {
            return BadRequest(new
            {
                code = "compliance_blocked",
                message = blocked.Message
            });
        }

        return BadRequest(new
        {
            message = "Unable to place order. Add a delivery address and check cart/stock."
        });
    }

    private Guid? GetCustomerUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}

public record PlaceOrderRequest(
    Guid CartId,
    string CustomerEmail,
    string CustomerFullName,
    Guid DeliveryAddressId);
