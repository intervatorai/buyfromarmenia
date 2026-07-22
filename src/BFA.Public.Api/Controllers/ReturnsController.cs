using System.Security.Claims;
using BFA.Public.Application.Commands.Returns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateReturnRequest(
        [FromBody] CreateReturnRequestBody request,
        CancellationToken cancellationToken)
    {
        Guid? customerUserId = null;
        if (User.Identity?.IsAuthenticated == true
            && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            customerUserId = userId;
        }

        var result = await _mediator.Send(
            new CreateReturnRequestCommand(
                request.CustomerOrderId,
                request.CustomerEmail,
                request.Reason,
                customerUserId),
            cancellationToken);

        return result is null
            ? BadRequest("Unable to create return request. Check order and email.")
            : CreatedAtAction(nameof(CreateReturnRequest), new { id = result.ReturnRequestId }, result);
    }
}

public record CreateReturnRequestBody(
    Guid CustomerOrderId,
    string CustomerEmail,
    string Reason);
