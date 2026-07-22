using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Suppliers;

public record GetSupplierQuery(Guid SupplierId) : IRequest<SupplierDetailDto?>;

public record SupplierDetailDto(
    Guid Id,
    string LegalName,
    string TradingName,
    string? TaxNumber,
    string? RegistrationNumber,
    string Status,
    string ContactPerson,
    string Email,
    string Phone,
    AddressDto? LegalAddress,
    AddressDto? WarehouseAddress,
    int PreparationDays,
    IReadOnlyList<SupplierBankAccountDto> BankAccounts,
    IReadOnlyList<SupplierDocumentDto> Documents,
    DateTime CreatedAt);

public record AddressDto(
    string CountryCode,
    string City,
    string Line1,
    string? Line2,
    string? PostalCode,
    string? Region);

public record SupplierBankAccountDto(
    Guid Id,
    string BankName,
    string AccountHolder,
    string Iban,
    string Currency,
    string? Swift,
    bool IsPrimary);

public record SupplierDocumentDto(
    Guid Id,
    string DocumentType,
    string FileName,
    string FileUrl,
    string Status,
    DateTime UploadedAt);

public class GetSupplierQueryHandler : IRequestHandler<GetSupplierQuery, SupplierDetailDto?>
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
        return supplier is null ? null : Map(supplier);
    }

    internal static SupplierDetailDto Map(BFA.Modules.Suppliers.Domain.Aggregates.Supplier supplier)
    {
        return new SupplierDetailDto(
            supplier.Id,
            supplier.LegalName,
            supplier.TradingName,
            supplier.TaxNumber,
            supplier.RegistrationNumber,
            supplier.Status.ToString(),
            supplier.Contact.ContactPerson,
            supplier.Contact.Email,
            supplier.Contact.Phone,
            MapAddress(supplier.LegalAddress),
            MapAddress(supplier.WarehouseAddress),
            (int)supplier.DefaultPreparationTime.TotalDays,
            supplier.BankAccounts.Select(a => new SupplierBankAccountDto(
                a.Id,
                a.Details.BankName,
                a.Details.AccountHolder,
                a.Details.Iban,
                a.Details.Currency,
                a.Details.Swift,
                a.IsPrimary)).ToList(),
            supplier.Documents.Select(d => new SupplierDocumentDto(
                d.Id,
                d.DocumentType.ToString(),
                d.FileName,
                d.FileUrl,
                d.Status.ToString(),
                d.UploadedAt)).ToList(),
            supplier.CreatedAt);
    }

    private static AddressDto? MapAddress(BFA.BuildingBlocks.Domain.Address? address)
    {
        if (address is null)
        {
            return null;
        }

        return new AddressDto(
            address.CountryCode,
            address.City,
            address.Line1,
            address.Line2,
            address.PostalCode,
            address.Region);
    }
}
