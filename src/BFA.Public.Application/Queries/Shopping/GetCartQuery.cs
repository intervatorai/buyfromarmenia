using BFA.Modules.Shopping.Domain.Aggregates;
using BFA.Modules.Shopping.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.Shopping;

public record GetCartQuery(Guid CartId) : IRequest<PublicCartDto>;

public record PublicCartDto(
    Guid Id,
    IReadOnlyList<PublicCartItemDto> Items,
    IReadOnlyList<Guid> WishlistProductIds,
    int TotalQuantity,
    decimal Subtotal,
    string Currency);

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

    public GetCartQueryHandler(IShoppingCartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<PublicCartDto> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByIdAsync(
            request.CartId,
            cancellationToken);
        return Map(cart ?? new ShoppingCart(request.CartId));
    }

    public static PublicCartDto Map(ShoppingCart cart)
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
            currency);
    }
}
