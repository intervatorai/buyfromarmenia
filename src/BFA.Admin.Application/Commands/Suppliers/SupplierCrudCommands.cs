using BFA.BuildingBlocks.Application;
using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using MediatR;

namespace BFA.Admin.Application.Commands.Suppliers;

public record CreateSupplierCommand(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber = null,
    string? RegistrationNumber = null,
    bool ActivateImmediately = false) : IRequest<Guid>;

public sealed class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Guid>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public CreateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Guid> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Register(
            request.LegalName,
            request.TradingName,
            new ContactInformation(request.ContactPerson, request.Email, request.Phone),
            taxNumber: request.TaxNumber,
            registrationNumber: request.RegistrationNumber);

        if (request.ActivateImmediately)
        {
            supplier.SubmitApplication();
            supplier.MarkUnderReview();
            supplier.Approve();
        }

        await _supplierRepository.AddAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            request.ActivateImmediately ? "SupplierCreatedAndActivated" : "SupplierCreated",
            "Supplier",
            supplier.Id,
            cancellationToken: cancellationToken);

        return supplier.Id;
    }
}

public record UpdateSupplierCommand(
    Guid SupplierId,
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber = null,
    string? RegistrationNumber = null) : IRequest<bool>;

public sealed class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public UpdateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        supplier.UpdateProfile(
            request.LegalName,
            request.TradingName,
            new ContactInformation(request.ContactPerson, request.Email, request.Phone),
            supplier.LegalAddress,
            request.TaxNumber,
            request.RegistrationNumber);

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SupplierUpdated",
            "Supplier",
            supplier.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
