using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Suppliers;

public record RejectSupplierCommand(Guid SupplierId, string Reason) : IRequest<bool>;

public class RejectSupplierCommandHandler : IRequestHandler<RejectSupplierCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;

    public RejectSupplierCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<bool> Handle(RejectSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        supplier.Reject(request.Reason);
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        return true;
    }
}
