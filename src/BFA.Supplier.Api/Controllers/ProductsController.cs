using BFA.Supplier.Application.Commands.Products;
using BFA.Supplier.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

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
    public async Task<IActionResult> GetProducts([FromQuery] Guid supplierId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductsQuery(supplierId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(
        Guid id,
        [FromQuery] Guid supplierId,
        CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(new GetProductQuery(supplierId, id), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var translations = ResolveTranslations(request);
        var productId = await _mediator.Send(
            new CreateProductCommand(
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
                request.NetWeight,
                request.GrossWeight,
                request.PackageLength,
                request.PackageWidth,
                request.PackageHeight,
                request.IsFragile,
                request.IsPerishable),
            cancellationToken);

        return productId is null
            ? BadRequest("Unable to create product. English name is required.")
            : CreatedAtAction(
                nameof(GetProduct),
                new { id = productId, supplierId = request.SupplierId },
                new { id = productId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var translations = ResolveTranslations(request);
        var updated = await _mediator.Send(
            new UpdateProductCommand(
                request.SupplierId,
                id,
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
                request.NetWeight,
                request.GrossWeight,
                request.PackageLength,
                request.PackageWidth,
                request.PackageHeight,
                request.IsFragile,
                request.IsPerishable),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        [FromQuery] Guid supplierId,
        CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(
            new DeleteProductCommand(supplierId, id),
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/variants")]
    public async Task<IActionResult> AddVariant(
        Guid id,
        [FromBody] AddVariantRequest request,
        CancellationToken cancellationToken)
    {
        var added = await _mediator.Send(
            new AddProductVariantCommand(
                request.SupplierId,
                id,
                request.SupplierSku,
                request.Weight,
                request.CountryOfOrigin,
                request.Barcode,
                request.Size,
                request.Color),
            cancellationToken);

        return added ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/media")]
    public async Task<IActionResult> AddMedia(
        Guid id,
        [FromBody] AddMediaRequest request,
        CancellationToken cancellationToken)
    {
        var added = await _mediator.Send(
            new AddProductMediaCommand(
                request.SupplierId,
                id,
                request.StorageKey,
                request.ContentType,
                request.AltText,
                request.IsPrimary,
                request.SortOrder),
            cancellationToken);

        return added ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/shipping")]
    public async Task<IActionResult> SetShipping(
        Guid id,
        [FromBody] SetShippingRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new SetProductShippingProfileCommand(
                request.SupplierId,
                id,
                request.NetWeight,
                request.GrossWeight,
                request.PackageLength,
                request.PackageWidth,
                request.PackageHeight,
                request.PackageDimensionUnit,
                request.IsFragile,
                request.IsPerishable),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitForReview(
        Guid id,
        [FromBody] SubmitProductRequest request,
        CancellationToken cancellationToken)
    {
        var submitted = await _mediator.Send(
            new SubmitProductForReviewCommand(request.SupplierId, id),
            cancellationToken);

        return submitted ? NoContent() : NotFound();
    }

    private static IReadOnlyList<ProductTranslationInput> ResolveTranslations(
        CreateProductRequest request)
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

    private static IReadOnlyList<ProductTranslationInput> ResolveTranslations(
        UpdateProductRequest request)
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

public record ProductTranslationRequest(
    string LanguageCode,
    string Name,
    string? ShortDescription = null,
    string? Description = null);

public record CreateProductRequest(
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
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? PackageLength = null,
    decimal? PackageWidth = null,
    decimal? PackageHeight = null,
    bool IsFragile = false,
    bool IsPerishable = false,
    IReadOnlyList<ProductTranslationRequest>? Translations = null);

public record UpdateProductRequest(
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
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? PackageLength = null,
    decimal? PackageWidth = null,
    decimal? PackageHeight = null,
    bool IsFragile = false,
    bool IsPerishable = false,
    IReadOnlyList<ProductTranslationRequest>? Translations = null);

public record AddVariantRequest(
    Guid SupplierId,
    string SupplierSku,
    decimal Weight,
    string CountryOfOrigin,
    string? Barcode = null,
    string? Size = null,
    string? Color = null);

public record AddMediaRequest(
    Guid SupplierId,
    string StorageKey,
    string ContentType = "image/jpeg",
    string? AltText = null,
    bool IsPrimary = false,
    int SortOrder = 0);

public record SetShippingRequest(
    Guid SupplierId,
    decimal NetWeight,
    decimal GrossWeight,
    decimal PackageLength,
    decimal PackageWidth,
    decimal PackageHeight,
    string PackageDimensionUnit = "cm",
    bool IsFragile = false,
    bool IsPerishable = false);

public record SubmitProductRequest(Guid SupplierId);
