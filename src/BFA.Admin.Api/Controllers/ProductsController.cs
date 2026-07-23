using BFA.Admin.Application.Commands.Products;
using BFA.Admin.Application.Queries.Products;
using BFA.BuildingBlocks.Domain;
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
        try
        {
            var updated = await _mediator.Send(
                new UpdateAdminProductCommand(
                    id,
                    request.Price,
                    request.Currency,
                    translations,
                    request.CategoryId,
                    request.Ingredients ?? "",
                    request.UsageInstructions ?? "",
                    ParseProductTag(request.Tag),
                    request.SupplierSku,
                    request.VariantWeight,
                    request.VariantSize,
                    request.VariantColor,
                    request.CountryOfOrigin ?? "AM"),
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/variants")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> AddVariant(
        Guid id,
        [FromBody] AdminProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var variantId = await _mediator.Send(
                new AddAdminProductVariantCommand(
                    id,
                    request.SupplierSku,
                    request.Weight,
                    request.CountryOfOrigin ?? "AM",
                    request.Size,
                    request.Color),
                cancellationToken);

            return variantId is null
                ? NotFound()
                : CreatedAtAction(nameof(GetProduct), new { id }, new { id = variantId });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/variants/{variantId:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateVariant(
        Guid id,
        Guid variantId,
        [FromBody] AdminProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _mediator.Send(
                new UpdateAdminProductVariantCommand(
                    id,
                    variantId,
                    request.SupplierSku ?? "",
                    request.Weight,
                    request.CountryOfOrigin ?? "AM",
                    request.Size,
                    request.Color),
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}/variants/{variantId:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> DeleteVariant(
        Guid id,
        Guid variantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _mediator.Send(
                new DeleteAdminProductVariantCommand(id, variantId),
                cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/shipping")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> SetShipping(
        Guid id,
        [FromBody] AdminProductShippingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _mediator.Send(
                new SetAdminProductShippingCommand(
                    id,
                    request.NetWeight,
                    request.GrossWeight,
                    request.PackageLength,
                    request.PackageWidth,
                    request.PackageHeight,
                    request.PackageDimensionUnit ?? "cm",
                    request.IsFragile,
                    request.IsPerishable),
                cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}/shipping")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> ClearShipping(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var cleared = await _mediator.Send(
                new ClearAdminProductShippingCommand(id),
                cancellationToken);
            return cleared ? NoContent() : NotFound();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
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
    string? SupplierSku = null,
    decimal? VariantWeight = null,
    string? VariantSize = null,
    string? VariantColor = null,
    string? CountryOfOrigin = "AM",
    IReadOnlyList<ProductTranslationRequest>? Translations = null);

public record AdminProductVariantRequest(
    string? SupplierSku,
    decimal Weight,
    string? Size = null,
    string? Color = null,
    string? CountryOfOrigin = "AM");

public record AdminProductShippingRequest(
    decimal NetWeight,
    decimal GrossWeight,
    decimal PackageLength,
    decimal PackageWidth,
    decimal PackageHeight,
    string? PackageDimensionUnit = "cm",
    bool IsFragile = false,
    bool IsPerishable = false);
