using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using MediatR;

namespace BFA.Supplier.Application.Commands.Suppliers;

public record AddSupplierBankAccountCommand(
    Guid SupplierId,
    string BankName,
    string AccountHolder,
    string Iban,
    string Currency,
    string? Swift = null,
    bool IsPrimary = true) : IRequest<bool>;

public class AddSupplierBankAccountCommandHandler
    : IRequestHandler<AddSupplierBankAccountCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;

    public AddSupplierBankAccountCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<bool> Handle(AddSupplierBankAccountCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        var details = new BankAccountDetails(
            request.BankName,
            request.AccountHolder,
            request.Iban,
            request.Currency,
            request.Swift);

        supplier.AddBankAccount(details, request.IsPrimary);
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        return true;
    }
}
