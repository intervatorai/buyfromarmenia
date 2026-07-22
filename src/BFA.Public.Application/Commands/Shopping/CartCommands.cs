using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Aggregates;
using BFA.Modules.Shopping.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Shopping;

public record AddCartItemCommand(
    Guid CartId,
    Guid ProductId,
    Guid ProductVariantId,
    int Quantity) : IRequest<bool>;

public sealed class AddCartItemCommandHandler
    : IRequestHandler<AddCartItemCommand, bool>
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IMediaUrlResolver _mediaUrlResolver;

    public AddCartItemCommandHandler(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository,
        IStockItemRepository stockItemRepository,
        IMediaUrlResolver mediaUrlResolver)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _stockItemRepository = stockItemRepository;
        _mediaUrlResolver = mediaUrlResolver;
    }

    public async Task<bool> Handle(
        AddCartItemCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return false;
        }

        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);
        var variant = product?.Variants.FirstOrDefault(item =>
            item.Id == request.ProductVariantId);

        if (product is null
            || product.Status != ProductStatus.Published
            || variant is null)
        {
            return false;
        }

        var stock = await _stockItemRepository.GetByVariantIdAsync(
            request.ProductVariantId,
            cancellationToken);
        var cart = await _cartRepository.GetByIdForUpdateAsync(
            request.CartId,
            cancellationToken);
        var currentQuantity = cart?.Items.FirstOrDefault(item =>
            item.ProductVariantId == request.ProductVariantId)?.Quantity ?? 0;

        if (stock is null || stock.Available < currentQuantity + request.Quantity)
        {
            return false;
        }

        var isNew = cart is null;
        cart ??= new ShoppingCart(request.CartId);
        var (name, _, _) = ProductDisplayHelper.GetDisplayText(product);
        var image = product.Media.FirstOrDefault(item => item.IsPrimary)
            ?? product.Media.OrderBy(item => item.SortOrder).FirstOrDefault();
        var imageUrl = image is null
            ? null
            : _mediaUrlResolver.Resolve(image.MediaAsset.StorageKey);

        cart.AddItem(
            product.Id,
            variant.Id,
            product.SupplierId,
            name,
            imageUrl,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            request.Quantity);

        if (isNew)
        {
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return true;
    }
}

public record ChangeCartItemQuantityCommand(
    Guid CartId,
    Guid ItemId,
    int Quantity) : IRequest<bool>;

public sealed class ChangeCartItemQuantityCommandHandler
    : IRequestHandler<ChangeCartItemQuantityCommand, bool>
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IStockItemRepository _stockItemRepository;

    public ChangeCartItemQuantityCommandHandler(
        IShoppingCartRepository cartRepository,
        IStockItemRepository stockItemRepository)
    {
        _cartRepository = cartRepository;
        _stockItemRepository = stockItemRepository;
    }

    public async Task<bool> Handle(
        ChangeCartItemQuantityCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return false;
        }

        var cart = await _cartRepository.GetByIdForUpdateAsync(
            request.CartId,
            cancellationToken);
        var item = cart?.Items.FirstOrDefault(item => item.Id == request.ItemId);
        if (cart is null || item is null)
        {
            return false;
        }

        var stock = await _stockItemRepository.GetByVariantIdAsync(
            item.ProductVariantId,
            cancellationToken);
        if (stock is null || stock.Available < request.Quantity)
        {
            return false;
        }

        cart.ChangeQuantity(request.ItemId, request.Quantity);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return true;
    }
}

public record RemoveCartItemCommand(Guid CartId, Guid ItemId) : IRequest<bool>;

public sealed class RemoveCartItemCommandHandler
    : IRequestHandler<RemoveCartItemCommand, bool>
{
    private readonly IShoppingCartRepository _cartRepository;

    public RemoveCartItemCommandHandler(IShoppingCartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<bool> Handle(
        RemoveCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByIdForUpdateAsync(
            request.CartId,
            cancellationToken);
        if (cart is null || cart.Items.All(item => item.Id != request.ItemId))
        {
            return false;
        }

        cart.RemoveItem(request.ItemId);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return true;
    }
}

public record SetWishlistProductCommand(
    Guid CartId,
    Guid ProductId,
    bool IsFavorite) : IRequest<bool>;

public sealed class SetWishlistProductCommandHandler
    : IRequestHandler<SetWishlistProductCommand, bool>
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public SetWishlistProductCommandHandler(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(
        SetWishlistProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);
        if (product is null || product.Status != ProductStatus.Published)
        {
            return false;
        }

        var cart = await _cartRepository.GetByIdForUpdateAsync(
            request.CartId,
            cancellationToken);
        var isNew = cart is null;
        cart ??= new ShoppingCart(request.CartId);

        if (request.IsFavorite)
        {
            cart.AddToWishlist(request.ProductId);
        }
        else
        {
            cart.RemoveFromWishlist(request.ProductId);
        }

        if (isNew)
        {
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return true;
    }
}
