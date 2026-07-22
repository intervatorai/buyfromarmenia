using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Suppliers;

public record GetSupplierQuery(Guid SupplierId) : IRequest<SupplierDetailDto?>;

public record SupplierDetailDto(
    Guid Id,
    string LegalName,
    string TradingName,
    string Status,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber,
    string? RegistrationNumber,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<SupplierDocumentDto> Documents,
    IReadOnlyList<SupplierBankAccountDto> BankAccounts);

public record SupplierDocumentDto(
    Guid Id,
    string DocumentType,
    string FileName,
    string FileUrl,
    string Status,
    DateTime UploadedAt);

public record SupplierBankAccountDto(
    Guid Id,
    string BankName,
    string AccountHolder,
    string Iban,
    string? Swift,
    string Currency,
    bool IsPrimary);

public sealed class GetSupplierQueryHandler : IRequestHandler<GetSupplierQuery, SupplierDetailDto?>
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierDetailDto?> Handle(
        GetSupplierQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return null;
        }

        return new SupplierDetailDto(
            supplier.Id,
            supplier.LegalName,
            supplier.TradingName,
            supplier.Status.ToString(),
            supplier.Contact.ContactPerson,
            supplier.Contact.Email,
            supplier.Contact.Phone,
            supplier.TaxNumber,
            supplier.RegistrationNumber,
            supplier.CreatedAt,
            supplier.UpdatedAt,
            supplier.Documents.Select(document => new SupplierDocumentDto(
                document.Id,
                document.DocumentType.ToString(),
                document.FileName,
                document.FileUrl,
                document.Status.ToString(),
                document.UploadedAt)).ToList(),
            supplier.BankAccounts.Select(account => new SupplierBankAccountDto(
                account.Id,
                account.Details.BankName,
                account.Details.AccountHolder,
                account.Details.Iban,
                account.Details.Swift,
                account.Details.Currency,
                account.IsPrimary)).ToList());
    }
}
