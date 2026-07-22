using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Products;

public record RequestProductChangesCommand(Guid ProductId, string Reason) : IRequest<bool>;

public class RequestProductChangesCommandHandler : IRequestHandler<RequestProductChangesCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public RequestProductChangesCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(RequestProductChangesCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.RequestChanges(request.Reason);
        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
