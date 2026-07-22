using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record SubmitProductForReviewCommand(Guid SupplierId, Guid ProductId) : IRequest<bool>;

public class SubmitProductForReviewCommandHandler : IRequestHandler<SubmitProductForReviewCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public SubmitProductForReviewCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(SubmitProductForReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);

        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        product.SubmitForReview();

        await _productRepository.UpdateAsync(product, cancellationToken);

        return true;
    }
}
