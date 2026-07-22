using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Enums;

namespace BFA.Modules.Catalog.Domain.Aggregates;

public sealed class ProductDocument : Entity
{
    public Guid ProductId { get; private set; }
    public ProductDocumentType DocumentType { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public DateTime? IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private ProductDocument()
    {
    }

    internal ProductDocument(
        Guid productId,
        ProductDocumentType documentType,
        string fileName,
        string fileUrl,
        DateTime? issuedAt = null,
        DateTime? expiresAt = null)
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
        ProductId = productId;
        DocumentType = documentType;
        FileName = fileName.Trim();
        FileUrl = fileUrl.Trim();
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    internal void Update(
        ProductDocumentType documentType,
        string fileName,
        string fileUrl,
        DateTime? issuedAt,
        DateTime? expiresAt)
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
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }
}
