using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Products;

public record RejectProductCommand(Guid ProductId, string Reason) : IRequest<bool>;

public class RejectProductCommandHandler : IRequestHandler<RejectProductCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public RejectProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(RejectProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Reject(request.Reason);
        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
