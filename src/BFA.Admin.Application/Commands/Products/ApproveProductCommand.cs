using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Products;

public record ApproveProductCommand(Guid ProductId) : IRequest<bool>;

public class ApproveProductCommandHandler : IRequestHandler<ApproveProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly IOutboxStore _outboxStore;

    public ApproveProductCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger,
        IOutboxStore outboxStore)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
        _outboxStore = outboxStore;
    }

    public async Task<bool> Handle(ApproveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return false;
        }

        product.Approve();
        product.Publish();

        await _productRepository.UpdateAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductApproved",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);

        await _outboxStore.EnqueueAsync(
            IntegrationEventTypes.ProductApproved,
            $"{{\"productId\":\"{product.Id}\",\"supplierId\":\"{product.SupplierId}\"}}",
            cancellationToken);

        return true;
    }
}
