using System.Security.Claims;
using BFA.Admin.Application.Commands.Customers;
using BFA.Admin.Application.Queries.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var customers = await _mediator.Send(new GetCustomersQuery(status), cancellationToken);
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _mediator.Send(new GetCustomerQuery(id), cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("{id:guid}/orders")]
    public async Task<IActionResult> GetCustomerOrders(Guid id, CancellationToken cancellationToken)
    {
        var orders = await _mediator.Send(new GetCustomerOrdersByCustomerQuery(id), cancellationToken);
        return orders is null ? NotFound() : Ok(orders);
    }

    [HttpPost]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = await _mediator.Send(
            new CreateCustomerCommand(
                request.Email,
                request.Password,
                request.FullName,
                request.Phone),
            cancellationToken);

        return customerId is null
            ? BadRequest("Unable to create customer. Check email uniqueness and password (min 6 chars).")
            : CreatedAtAction(nameof(GetCustomer), new { id = customerId }, new { id = customerId });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new UpdateCustomerCommand(id, request.FullName, request.Phone, request.NewPassword),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        var suspended = await _mediator.Send(new SuspendCustomerCommand(id), cancellationToken);
        return suspended ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var activated = await _mediator.Send(new ActivateCustomerCommand(id), cancellationToken);
        return activated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/impersonate")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Impersonate(Guid id, CancellationToken cancellationToken)
    {
        var adminIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdValue, out var adminId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(
            new ImpersonateCustomerCommand(id, adminId),
            cancellationToken);

        return result is null
            ? BadRequest("Unable to impersonate customer. Account may be missing or suspended.")
            : Ok(result);
    }
}

public record CreateCustomerRequest(
    string Email,
    string Password,
    string FullName,
    string? Phone);

public record UpdateCustomerRequest(
    string FullName,
    string? Phone,
    string? NewPassword = null);
