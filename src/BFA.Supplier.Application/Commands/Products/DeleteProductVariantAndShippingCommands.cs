using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record DeleteProductVariantCommand(
    Guid SupplierId,
    Guid ProductId,
    Guid VariantId) : IRequest<bool>;

public sealed class DeleteProductVariantCommandHandler
    : IRequestHandler<DeleteProductVariantCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IShoppingCartRepository _cartRepository;

    public DeleteProductVariantCommandHandler(
        IProductRepository productRepository,
        IStockItemRepository stockItemRepository,
        IShoppingCartRepository cartRepository)
    {
        _productRepository = productRepository;
        _stockItemRepository = stockItemRepository;
        _cartRepository = cartRepository;
    }

    public async Task<bool> Handle(
        DeleteProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        var stock = await _stockItemRepository.GetByVariantIdForUpdateAsync(
            request.VariantId,
            cancellationToken);
        if (stock is not null)
        {
            if (stock.Reserved > 0)
            {
                throw new DomainException(
                    "Cannot delete this variant while stock is reserved for open orders.");
            }

            await _stockItemRepository.DeleteAsync(stock, cancellationToken);
        }

        product.RemoveVariant(request.VariantId);
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _cartRepository.RemoveItemsByProductVariantIdAsync(
            request.VariantId,
            cancellationToken);
        return true;
    }
}

public record ClearProductShippingProfileCommand(
    Guid SupplierId,
    Guid ProductId) : IRequest<bool>;

public sealed class ClearProductShippingProfileCommandHandler
    : IRequestHandler<ClearProductShippingProfileCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public ClearProductShippingProfileCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(
        ClearProductShippingProfileCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);
        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        product.ClearShippingProfile();
        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
