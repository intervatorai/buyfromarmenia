using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record DeleteProductCommand(Guid SupplierId, Guid ProductId) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(
            request.ProductId,
            cancellationToken);

        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        if (product.Status is ProductStatus.Draft
            or ProductStatus.ChangesRequested
            or ProductStatus.Rejected)
        {
            await _productRepository.DeleteAsync(product, cancellationToken);
            return true;
        }

        if (product.Status == ProductStatus.Archived)
        {
            return true;
        }

        product.Archive();
        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
