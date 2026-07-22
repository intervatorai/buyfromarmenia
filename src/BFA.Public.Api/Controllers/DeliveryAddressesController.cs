using System.Security.Claims;
using BFA.BuildingBlocks.Domain;
using BFA.Public.Application.Commands.DeliveryAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/delivery-addresses")]
public sealed class DeliveryAddressesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryAddressesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var addresses = await _mediator.Send(
            new GetCustomerDeliveryAddressesQuery(userId.Value),
            cancellationToken);
        return Ok(addresses);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] UpsertDeliveryAddressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var created = await _mediator.Send(
                new CreateCustomerDeliveryAddressCommand(
                    userId.Value,
                    request.CountryCode,
                    request.City,
                    request.Line1,
                    request.Line2,
                    request.PostalCode,
                    request.Region,
                    request.Label,
                    request.IsDefault),
                cancellationToken);

            return created is null
                ? NotFound()
                : CreatedAtAction(nameof(List), created);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpsertDeliveryAddressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _mediator.Send(
                new UpdateCustomerDeliveryAddressCommand(
                    userId.Value,
                    id,
                    request.CountryCode,
                    request.City,
                    request.Line1,
                    request.Line2,
                    request.PostalCode,
                    request.Region,
                    request.Label),
                cancellationToken);

            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var ok = await _mediator.Send(
            new SetDefaultCustomerDeliveryAddressCommand(userId.Value, id),
            cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var ok = await _mediator.Send(
            new DeleteCustomerDeliveryAddressCommand(userId.Value, id),
            cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}

public record UpsertDeliveryAddressRequest(
    string CountryCode,
    string City,
    string Line1,
    string? Line2 = null,
    string? PostalCode = null,
    string? Region = null,
    string? Label = null,
    bool IsDefault = false);
