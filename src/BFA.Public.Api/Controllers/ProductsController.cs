using BFA.Public.Application.Commands.Products;
using BFA.Public.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] string? tag,
        [FromQuery] bool featuredOnly = false,
        [FromQuery] int? take = null,
        [FromQuery] string? lang = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(categoryId, category, search, tag, featuredOnly, take, lang),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{slugOrId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(
        string slugOrId,
        [FromQuery] string? lang = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProductQuery(slugOrId, lang), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.SupplierId,
            request.Name,
            request.Description,
            request.Price,
            request.Currency);

        var productId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProducts), new { id = productId }, new { id = productId });
    }
}

public record CreateProductRequest(
    Guid SupplierId,
    string Name,
    string Description,
    decimal Price,
    string Currency);
