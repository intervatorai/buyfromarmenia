using BFA.Admin.Application.Commands.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public sealed class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize(Policy = "ModeratorOrAbove")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] Guid? productId,
        [FromForm] bool isPrimary = true,
        [FromForm] string? altText = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "File is required." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _mediator.Send(
                new UploadProductImageCommand(
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    productId,
                    isPrimary,
                    altText),
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
