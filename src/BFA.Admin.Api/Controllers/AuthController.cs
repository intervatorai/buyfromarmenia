using System.Security.Claims;
using BFA.Admin.Application.Commands.Auth;
using BFA.Admin.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        if (result is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentAdmin(CancellationToken cancellationToken)
    {
        var adminIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(adminIdValue, out var adminId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetCurrentAdminQuery(adminId), cancellationToken);

        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }
}

public record LoginRequest(string Email, string Password);
