using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Products;

public record SuspendProductCommand(Guid ProductId) : IRequest<bool>;

public sealed class SuspendProductCommandHandler : IRequestHandler<SuspendProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;

    public SuspendProductCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(SuspendProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Suspend();
        await _productRepository.UpdateAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductSuspended",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record ArchiveProductCommand(Guid ProductId) : IRequest<bool>;

public sealed class ArchiveProductCommandHandler : IRequestHandler<ArchiveProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;

    public ArchiveProductCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Archive();
        await _productRepository.UpdateAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductArchived",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record RepublishProductCommand(Guid ProductId) : IRequest<bool>;

public sealed class RepublishProductCommandHandler : IRequestHandler<RepublishProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IAuditLogger _auditLogger;

    public RepublishProductCommandHandler(
        IProductRepository productRepository,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(RepublishProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Publish();
        await _productRepository.UpdateAsync(product, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ProductRepublished",
            "Product",
            product.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
