using System.Security.Claims;
using BFA.Supplier.Application.Commands.Auth;
using BFA.Supplier.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginSupplierCommand(request.Email, request.Password),
            cancellationToken);

        return result is null
            ? Unauthorized(new { message = "Invalid email or password." })
            : Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentSupplier(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetCurrentSupplierQuery(userId), cancellationToken);
        return result is null ? Unauthorized() : Ok(result);
    }
}

public record LoginSupplierRequest(string Email, string Password);
