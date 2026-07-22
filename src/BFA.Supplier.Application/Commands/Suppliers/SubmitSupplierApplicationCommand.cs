using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Suppliers;

public record SubmitSupplierApplicationCommand(Guid SupplierId) : IRequest<bool>;

public class SubmitSupplierApplicationCommandHandler
    : IRequestHandler<SubmitSupplierApplicationCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;

    public SubmitSupplierApplicationCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<bool> Handle(
        SubmitSupplierApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        if (supplier.BankAccounts.Count == 0)
        {
            throw new InvalidOperationException("Add a bank account before submitting the application.");
        }

        supplier.SubmitApplication();
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        return true;
    }
}
