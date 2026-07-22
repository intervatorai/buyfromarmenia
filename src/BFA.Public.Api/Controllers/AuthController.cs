using System.Security.Claims;
using BFA.Public.Application.Commands.Auth;
using BFA.Public.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RegisterCustomerCommand(
                request.Email,
                request.Password,
                request.FullName,
                request.Phone),
            cancellationToken);

        return result is null
            ? Conflict(new { message = "Email is already registered." })
            : Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCustomerCommand(request.Email, request.Password),
            cancellationToken);

        return result is null
            ? Unauthorized(new { message = "Invalid email or password." })
            : Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentCustomer(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetCurrentCustomerQuery(userId), cancellationToken);
        return result is null ? Unauthorized() : Ok(result);
    }
}

public record RegisterCustomerRequest(
    string Email,
    string Password,
    string FullName,
    string? Phone);

public record LoginCustomerRequest(string Email, string Password);
