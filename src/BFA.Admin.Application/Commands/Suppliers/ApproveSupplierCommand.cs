using BFA.BuildingBlocks.Application;
using BFA.Modules.Suppliers.Domain.Enums;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Suppliers;

public record ApproveSupplierCommand(Guid SupplierId) : IRequest<bool>;

public class ApproveSupplierCommandHandler : IRequestHandler<ApproveSupplierCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public ApproveSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(ApproveSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        if (supplier.Status == SupplierStatus.ApplicationSubmitted)
        {
            supplier.MarkUnderReview();
        }

        supplier.Approve();
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SupplierApproved",
            "Supplier",
            supplier.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
