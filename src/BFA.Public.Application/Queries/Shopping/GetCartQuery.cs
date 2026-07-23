using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Aggregates;
using BFA.Modules.Shopping.Domain.Repositories;
using BFA.Public.Application.Services.Shopping;
using MediatR;

namespace BFA.Public.Application.Queries.Shopping;

public record GetCartQuery(Guid CartId) : IRequest<PublicCartDto>;

public record PublicCartDto(
    Guid Id,
    IReadOnlyList<PublicCartItemDto> Items,
    IReadOnlyList<Guid> WishlistProductIds,
    int TotalQuantity,
    decimal Subtotal,
    string Currency,
    int RemovedUnavailableItems = 0);

public record PublicCartItemDto(
    Guid Id,
    Guid ProductId,
    Guid ProductVariantId,
    Guid SupplierId,
    string ProductName,
    string? ImageUrl,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);

public sealed class GetCartQueryHandler
    : IRequestHandler<GetCartQuery, PublicCartDto>
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public GetCartQueryHandler(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<PublicCartDto> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByIdForUpdateAsync(
            request.CartId,
            cancellationToken);
        if (cart is null)
        {
            return Map(new ShoppingCart(request.CartId));
        }

        var removed = await CartCatalogSanitizer.RemoveUnavailableItemsAsync(
            cart,
            _productRepository,
            cancellationToken);
        if (removed > 0)
        {
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return Map(cart, removed);
    }

    public static PublicCartDto Map(ShoppingCart cart, int removedUnavailableItems = 0)
    {
        var items = cart.Items.Select(item => new PublicCartItemDto(
            item.Id,
            item.ProductId,
            item.ProductVariantId,
            item.SupplierId,
            item.ProductName,
            item.ImageUrl,
            item.UnitPrice,
            item.Currency,
            item.Quantity,
            item.LineTotal)).ToList();

        var currency = items.Select(item => item.Currency).Distinct().Count() == 1
            ? items.FirstOrDefault()?.Currency ?? "USD"
            : "USD";

        return new PublicCartDto(
            cart.Id,
            items,
            cart.WishlistItems.Select(item => item.ProductId).ToList(),
            items.Sum(item => item.Quantity),
            items.Sum(item => item.LineTotal),
            currency,
            removedUnavailableItems);
    }
}
