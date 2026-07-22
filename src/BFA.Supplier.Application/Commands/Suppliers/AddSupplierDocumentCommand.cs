using BFA.Modules.Suppliers.Domain.Enums;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Suppliers;

public record AddSupplierDocumentCommand(
    Guid SupplierId,
    string DocumentType,
    string FileName,
    string FileUrl) : IRequest<bool>;

public class AddSupplierDocumentCommandHandler : IRequestHandler<AddSupplierDocumentCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;

    public AddSupplierDocumentCommandHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<bool> Handle(AddSupplierDocumentCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        if (!Enum.TryParse<SupplierDocumentType>(request.DocumentType, true, out var documentType))
        {
            throw new ArgumentException($"Unknown document type: {request.DocumentType}");
        }

        supplier.AddDocument(documentType, request.FileName, request.FileUrl);
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        return true;
    }
}
