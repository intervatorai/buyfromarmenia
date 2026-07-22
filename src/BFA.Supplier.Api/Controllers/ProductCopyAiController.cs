using BFA.BuildingBlocks.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BFA.Supplier.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products/ai")]
public sealed class ProductCopyAiController(IProductCopyGenerator generator) : ControllerBase
{
    [HttpGet("enabled")]
    public IActionResult Enabled() => Ok(new { enabled = generator.IsEnabled });

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(
        [FromBody] ProductCopyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await generator.GenerateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "OpenAI returned an invalid response. Please retry." });
        }
    }
}
