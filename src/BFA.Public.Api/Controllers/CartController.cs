using BFA.Public.Application.Commands.Shopping;
using BFA.Public.Application.Queries.Shopping;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Public.Api.Controllers;

[ApiController]
[Route("api/carts/{cartId:guid}")]
[AllowAnonymous]
public sealed class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(
        Guid cartId,
        CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(
            new GetCartQuery(cartId),
            cancellationToken));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        Guid cartId,
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var added = await _mediator.Send(
            new AddCartItemCommand(
                cartId,
                request.ProductId,
                request.ProductVariantId,
                request.Quantity),
            cancellationToken);
        return added ? NoContent() : BadRequest("Product is unavailable.");
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> ChangeQuantity(
        Guid cartId,
        Guid itemId,
        [FromBody] ChangeCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new ChangeCartItemQuantityCommand(cartId, itemId, request.Quantity),
            cancellationToken);
        return updated ? NoContent() : BadRequest("Requested quantity is unavailable.");
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid cartId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var removed = await _mediator.Send(
            new RemoveCartItemCommand(cartId, itemId),
            cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    [HttpPut("wishlist/{productId:guid}")]
    public async Task<IActionResult> SetWishlistProduct(
        Guid cartId,
        Guid productId,
        [FromBody] SetWishlistProductRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new SetWishlistProductCommand(cartId, productId, request.IsFavorite),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}

public record AddCartItemRequest(
    Guid ProductId,
    Guid ProductVariantId,
    int Quantity = 1);

public record ChangeCartItemQuantityRequest(int Quantity);
public record SetWishlistProductRequest(bool IsFavorite);
