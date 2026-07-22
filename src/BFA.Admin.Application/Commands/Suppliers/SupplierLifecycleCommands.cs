using BFA.BuildingBlocks.Application;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Suppliers;

public record RequestSupplierChangesCommand(Guid SupplierId, string Reason) : IRequest<bool>;

public sealed class RequestSupplierChangesCommandHandler
    : IRequestHandler<RequestSupplierChangesCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public RequestSupplierChangesCommandHandler(
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        RequestSupplierChangesCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        supplier.RequestChanges(request.Reason);
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SupplierChangesRequested",
            "Supplier",
            supplier.Id,
            $"{{\"reason\":\"{request.Reason}\"}}",
            cancellationToken);

        return true;
    }
}

public record SuspendSupplierCommand(Guid SupplierId, string Reason) : IRequest<bool>;

public sealed class SuspendSupplierCommandHandler : IRequestHandler<SuspendSupplierCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public SuspendSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(SuspendSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        supplier.Suspend(request.Reason);
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SupplierSuspended",
            "Supplier",
            supplier.Id,
            $"{{\"reason\":\"{request.Reason}\"}}",
            cancellationToken);

        return true;
    }
}

public record ActivateSupplierCommand(Guid SupplierId) : IRequest<bool>;

public sealed class ActivateSupplierCommandHandler : IRequestHandler<ActivateSupplierCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public ActivateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(ActivateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        supplier.Activate();
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SupplierActivated",
            "Supplier",
            supplier.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
