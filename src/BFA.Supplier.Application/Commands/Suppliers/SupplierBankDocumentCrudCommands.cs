using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.Enums;
using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using MediatR;

namespace BFA.Supplier.Application.Commands.Suppliers;

public record SupplierMutationResult(bool Success, string? Error = null);

public record UpdateSupplierBankAccountCommand(
    Guid SupplierId,
    Guid BankAccountId,
    string BankName,
    string AccountHolder,
    string Iban,
    string Currency,
    string? Swift = null,
    bool IsPrimary = true) : IRequest<SupplierMutationResult>;

public record RemoveSupplierBankAccountCommand(
    Guid SupplierId,
    Guid BankAccountId) : IRequest<SupplierMutationResult>;

public record UpdateSupplierDocumentCommand(
    Guid SupplierId,
    Guid DocumentId,
    string DocumentType,
    string FileName,
    string FileUrl) : IRequest<SupplierMutationResult>;

public record RemoveSupplierDocumentCommand(
    Guid SupplierId,
    Guid DocumentId) : IRequest<SupplierMutationResult>;

public sealed class UpdateSupplierBankAccountCommandHandler
    : IRequestHandler<UpdateSupplierBankAccountCommand, SupplierMutationResult>
{
    private readonly ISupplierRepository _supplierRepository;

    public UpdateSupplierBankAccountCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierMutationResult> Handle(
        UpdateSupplierBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return new SupplierMutationResult(false, "Supplier not found.");
        }

        try
        {
            var details = new BankAccountDetails(
                request.BankName,
                request.AccountHolder,
                request.Iban,
                request.Currency,
                request.Swift);
            supplier.UpdateBankAccount(request.BankAccountId, details, request.IsPrimary);
            await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        }
        catch (DomainException ex)
        {
            return new SupplierMutationResult(false, ex.Message);
        }

        return new SupplierMutationResult(true);
    }
}

public sealed class RemoveSupplierBankAccountCommandHandler
    : IRequestHandler<RemoveSupplierBankAccountCommand, SupplierMutationResult>
{
    private readonly ISupplierRepository _supplierRepository;

    public RemoveSupplierBankAccountCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierMutationResult> Handle(
        RemoveSupplierBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return new SupplierMutationResult(false, "Supplier not found.");
        }

        try
        {
            supplier.RemoveBankAccount(request.BankAccountId);
            await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        }
        catch (DomainException ex)
        {
            return new SupplierMutationResult(false, ex.Message);
        }

        return new SupplierMutationResult(true);
    }
}

public sealed class UpdateSupplierDocumentCommandHandler
    : IRequestHandler<UpdateSupplierDocumentCommand, SupplierMutationResult>
{
    private readonly ISupplierRepository _supplierRepository;

    public UpdateSupplierDocumentCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierMutationResult> Handle(
        UpdateSupplierDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return new SupplierMutationResult(false, "Supplier not found.");
        }

        if (!Enum.TryParse<SupplierDocumentType>(request.DocumentType, true, out var documentType))
        {
            return new SupplierMutationResult(false, "Unknown document type.");
        }

        try
        {
            supplier.UpdateDocument(
                request.DocumentId,
                documentType,
                request.FileName,
                request.FileUrl);
            await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        }
        catch (DomainException ex)
        {
            return new SupplierMutationResult(false, ex.Message);
        }

        return new SupplierMutationResult(true);
    }
}

public sealed class RemoveSupplierDocumentCommandHandler
    : IRequestHandler<RemoveSupplierDocumentCommand, SupplierMutationResult>
{
    private readonly ISupplierRepository _supplierRepository;

    public RemoveSupplierDocumentCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierMutationResult> Handle(
        RemoveSupplierDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return new SupplierMutationResult(false, "Supplier not found.");
        }

        try
        {
            supplier.RemoveDocument(request.DocumentId);
            await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        }
        catch (DomainException ex)
        {
            return new SupplierMutationResult(false, ex.Message);
        }

        return new SupplierMutationResult(true);
    }
}
