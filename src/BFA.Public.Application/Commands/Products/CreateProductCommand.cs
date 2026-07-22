using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Products;

public record CreateProductCommand(
    Guid SupplierId,
    string Name,
    string Description,
    decimal Price,
    string Currency) : IRequest<Guid>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.SupplierId,
            new Money(request.Price, request.Currency),
            request.Name,
            request.Description);

        product.SubmitForReview();
        await ProductSlugAssigner.AssignUniqueSlugAsync(product, _productRepository, cancellationToken);

        await _productRepository.AddAsync(product, cancellationToken);

        return product.Id;
    }
}
