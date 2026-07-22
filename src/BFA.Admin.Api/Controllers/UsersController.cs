using BFA.Admin.Application.Commands.Users;
using BFA.Admin.Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetAdminUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateAdminUserCommand(
                request.Email,
                request.Password,
                request.FullName,
                request.Role),
            cancellationToken);

        return id is null
            ? BadRequest("Unable to create admin user. Check email uniqueness, role, and password.")
            : CreatedAtAction(nameof(GetUsers), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new UpdateAdminUserCommand(id, request.FullName, request.Role),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
    {
        var suspended = await _mediator.Send(new SuspendAdminUserCommand(id), cancellationToken);
        return suspended ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var activated = await _mediator.Send(new ActivateAdminUserCommand(id), cancellationToken);
        return activated ? NoContent() : NotFound();
    }
}

public record CreateAdminUserRequest(string Email, string Password, string FullName, string Role);

public record UpdateAdminUserRequest(string FullName, string Role);
