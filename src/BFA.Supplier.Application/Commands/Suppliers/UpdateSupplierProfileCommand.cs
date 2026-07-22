using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using MediatR;

namespace BFA.Supplier.Application.Commands.Suppliers;

public record UpdateSupplierProfileCommand(
    Guid SupplierId,
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber,
    string? RegistrationNumber,
    string? LegalCountryCode,
    string? LegalCity,
    string? LegalLine1,
    string? LegalLine2,
    string? LegalPostalCode,
    string? LegalRegion,
    string? WarehouseCountryCode,
    string? WarehouseCity,
    string? WarehouseLine1,
    string? WarehouseLine2,
    string? WarehousePostalCode,
    string? WarehouseRegion,
    int PreparationDays = 2) : IRequest<bool>;

public class UpdateSupplierProfileCommandHandler : IRequestHandler<UpdateSupplierProfileCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;

    public UpdateSupplierProfileCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<bool> Handle(UpdateSupplierProfileCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        var contact = new ContactInformation(request.ContactPerson, request.Email, request.Phone);
        var legalAddress = TryCreateAddress(
            request.LegalCountryCode,
            request.LegalCity,
            request.LegalLine1,
            request.LegalLine2,
            request.LegalPostalCode,
            request.LegalRegion);

        supplier.UpdateProfile(
            request.LegalName,
            request.TradingName,
            contact,
            legalAddress,
            request.TaxNumber,
            request.RegistrationNumber);

        supplier.SetWarehouseAddress(TryCreateAddress(
            request.WarehouseCountryCode,
            request.WarehouseCity,
            request.WarehouseLine1,
            request.WarehouseLine2,
            request.WarehousePostalCode,
            request.WarehouseRegion));

        supplier.SetDefaultPreparationTime(TimeSpan.FromDays(request.PreparationDays));

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        return true;
    }

    private static Address? TryCreateAddress(
        string? countryCode,
        string? city,
        string? line1,
        string? line2,
        string? postalCode,
        string? region)
    {
        if (string.IsNullOrWhiteSpace(countryCode)
            || string.IsNullOrWhiteSpace(city)
            || string.IsNullOrWhiteSpace(line1))
        {
            return null;
        }

        return new Address(countryCode, city, line1, line2, postalCode, region);
    }
}
