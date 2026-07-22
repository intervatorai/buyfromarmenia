using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record AddProductMediaCommand(
    Guid SupplierId,
    Guid ProductId,
    string StorageKey,
    string ContentType = "image/jpeg",
    string? AltText = null,
    bool IsPrimary = false,
    int SortOrder = 0) : IRequest<bool>;

public class AddProductMediaCommandHandler : IRequestHandler<AddProductMediaCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public AddProductMediaCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(AddProductMediaCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        var asset = MediaAsset.Create(request.StorageKey, request.ContentType);
        product.AddMedia(asset, request.AltText, request.SortOrder, request.IsPrimary);
        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}

public record SetPrimaryProductMediaCommand(
    Guid SupplierId,
    Guid ProductId,
    Guid ProductMediaId) : IRequest<bool>;

public class SetPrimaryProductMediaCommandHandler : IRequestHandler<SetPrimaryProductMediaCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public SetPrimaryProductMediaCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(SetPrimaryProductMediaCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        try
        {
            product.SetPrimaryMedia(request.ProductMediaId);
        }
        catch (BuildingBlocks.Domain.DomainException)
        {
            return false;
        }

        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
