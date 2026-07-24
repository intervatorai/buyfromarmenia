using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.Enums;

namespace BFA.Modules.Suppliers.Domain.Aggregates;

public sealed class SupplierDocument : Entity
{
    public Guid SupplierId { get; private set; }
    public SupplierDocumentType DocumentType { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public SupplierDocumentStatus Status { get; private set; } = SupplierDocumentStatus.Pending;
    public DateTime UploadedAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    private SupplierDocument()
    {
    }

    internal SupplierDocument(
        Guid supplierId,
        SupplierDocumentType documentType,
        string fileName,
        string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new DomainException("Document file name is required.");
        }

        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new DomainException("Document file URL is required.");
        }

        Id = Guid.NewGuid();
        SupplierId = supplierId;
        DocumentType = documentType;
        FileName = fileName.Trim();
        FileUrl = fileUrl.Trim();
        UploadedAt = DateTime.UtcNow;
    }

    internal void Verify()
    {
        Status = SupplierDocumentStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
    }

    internal void Update(SupplierDocumentType documentType, string fileName, string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new DomainException("Document file name is required.");
        }

        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new DomainException("Document file URL is required.");
        }

        DocumentType = documentType;
        FileName = fileName.Trim();
        FileUrl = fileUrl.Trim();
        Status = SupplierDocumentStatus.Pending;
        VerifiedAt = null;
    }

    internal void Reject()
    {
        Status = SupplierDocumentStatus.Rejected;
        VerifiedAt = null;
    }

    internal void MarkExpired()
    {
        Status = SupplierDocumentStatus.Expired;
    }
}
