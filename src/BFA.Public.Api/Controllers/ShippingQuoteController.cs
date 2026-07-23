using System.Security.Claims;
using BFA.BuildingBlocks.Domain;
using BFA.Public.Application.Queries.Shipping;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Route("api/shipping")]
[Authorize]
public sealed class ShippingQuoteController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingQuoteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("quote")]
    public async Task<IActionResult> GetQuote(
        [FromQuery] Guid cartId,
        [FromQuery] Guid deliveryAddressId,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var customerUserId))
        {
            return Unauthorized();
        }

        try
        {
            var quote = await _mediator.Send(
                new GetShippingQuoteQuery(cartId, deliveryAddressId, customerUserId),
                cancellationToken);
            return Ok(quote);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
