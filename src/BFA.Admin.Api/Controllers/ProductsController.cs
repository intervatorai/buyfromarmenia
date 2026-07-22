using BFA.Admin.Application.Commands.Products;
using BFA.Admin.Application.Queries.Products;
using BFA.Modules.Catalog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductsQuery(status), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("rejection-templates")]
    public IActionResult GetRejectionTemplates()
    {
        return Ok(ProductRejectionTemplates.All);
    }

    [HttpPost]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateAdminProductRequest request,
        CancellationToken cancellationToken)
    {
        var translations = ResolveTranslations(request);
        var id = await _mediator.Send(
            new CreateAdminProductCommand(
                request.SupplierId,
                request.Price,
                request.Currency,
                translations,
                request.CategoryId,
                request.Ingredients ?? "",
                request.UsageInstructions ?? "",
                request.SupplierSku,
                request.VariantWeight,
                request.VariantSize,
                request.VariantColor,
                request.CountryOfOrigin ?? "AM",
                request.ImageStorageKey,
                request.PublishImmediately,
                ParseProductTag(request.Tag)),
            cancellationToken);

        return id is null
            ? BadRequest("Unable to create product. Check supplier id and fields.")
            : CreatedAtAction(nameof(GetProduct), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateAdminProductRequest request,
        CancellationToken cancellationToken)
    {
        var translations = ResolveTranslations(request);
        var updated = await _mediator.Send(
            new UpdateAdminProductCommand(
                id,
                request.Price,
                request.Currency,
                translations,
                request.CategoryId,
                request.Ingredients ?? "",
                request.UsageInstructions ?? "",
                ParseProductTag(request.Tag)),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteAdminProductCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> ApproveProduct(Guid id, CancellationToken cancellationToken)
    {
        var approved = await _mediator.Send(new ApproveProductCommand(id), cancellationToken);
        return approved ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> RejectProduct(
        Guid id,
        [FromBody] ProductModerationRequest request,
        CancellationToken cancellationToken)
    {
        var rejected = await _mediator.Send(new RejectProductCommand(id, request.Reason), cancellationToken);
        return rejected ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/request-changes")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> RequestProductChanges(
        Guid id,
        [FromBody] ProductModerationRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new RequestProductChangesCommand(id, request.Reason),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> SuspendProduct(Guid id, CancellationToken cancellationToken)
    {
        var suspended = await _mediator.Send(new SuspendProductCommand(id), cancellationToken);
        return suspended ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> ArchiveProduct(Guid id, CancellationToken cancellationToken)
    {
        var archived = await _mediator.Send(new ArchiveProductCommand(id), cancellationToken);
        return archived ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> RepublishProduct(Guid id, CancellationToken cancellationToken)
    {
        var published = await _mediator.Send(new RepublishProductCommand(id), cancellationToken);
        return published ? NoContent() : NotFound();
    }

    private static ProductTag ParseProductTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return ProductTag.None;
        }

        return Enum.TryParse<ProductTag>(tag, ignoreCase: true, out var parsed)
            ? parsed
            : ProductTag.None;
    }

    private static IReadOnlyList<ProductTranslationInput> ResolveTranslations(
        CreateAdminProductRequest request)
    {
        if (request.Translations is { Count: > 0 })
        {
            return request.Translations
                .Select(t => new ProductTranslationInput(
                    t.LanguageCode,
                    t.Name,
                    t.ShortDescription ?? "",
                    t.Description ?? ""))
                .ToList();
        }

        // Backward-compatible single-language payload.
        return
        [
            new ProductTranslationInput(
                request.LanguageCode ?? "en",
                request.Name ?? "",
                request.ShortDescription ?? "",
                request.Description ?? "")
        ];
    }

    private static IReadOnlyList<ProductTranslationInput> ResolveTranslations(
        UpdateAdminProductRequest request)
    {
        if (request.Translations is { Count: > 0 })
        {
            return request.Translations
                .Select(t => new ProductTranslationInput(
                    t.LanguageCode,
                    t.Name,
                    t.ShortDescription ?? "",
                    t.Description ?? ""))
                .ToList();
        }

        return
        [
            new ProductTranslationInput(
                request.LanguageCode ?? "en",
                request.Name ?? "",
                request.ShortDescription ?? "",
                request.Description ?? "")
        ];
    }
}

public record ProductModerationRequest(string Reason);

public record ProductTranslationRequest(
    string LanguageCode,
    string Name,
    string? ShortDescription = null,
    string? Description = null);

public record CreateAdminProductRequest(
    Guid SupplierId,
    decimal Price,
    string Currency,
    string? Name = null,
    string? Description = null,
    string? LanguageCode = "en",
    Guid? CategoryId = null,
    string? ShortDescription = null,
    string? Ingredients = null,
    string? UsageInstructions = null,
    string? SupplierSku = null,
    decimal? VariantWeight = null,
    string? VariantSize = null,
    string? VariantColor = null,
    string? CountryOfOrigin = "AM",
    string? ImageStorageKey = null,
    bool PublishImmediately = false,
    string? Tag = null,
    IReadOnlyList<ProductTranslationRequest>? Translations = null);

public record UpdateAdminProductRequest(
    decimal Price,
    string Currency,
    string? Name = null,
    string? Description = null,
    string? LanguageCode = "en",
    Guid? CategoryId = null,
    string? ShortDescription = null,
    string? Ingredients = null,
    string? UsageInstructions = null,
    string? Tag = null,
    IReadOnlyList<ProductTranslationRequest>? Translations = null);
