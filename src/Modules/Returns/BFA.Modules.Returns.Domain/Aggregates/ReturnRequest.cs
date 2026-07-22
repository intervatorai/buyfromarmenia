using BFA.BuildingBlocks.Domain;
using BFA.Modules.Returns.Domain.Enums;

namespace BFA.Modules.Returns.Domain.Aggregates;

public sealed class ReturnRequest : AggregateRoot
{
    public Guid CustomerOrderId { get; private set; }
    public Guid? CustomerUserId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public ReturnRequestStatus Status { get; private set; }
    public string? AdminNotes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    private ReturnRequest()
    {
    }

    public static ReturnRequest Create(
        Guid customerOrderId,
        string customerEmail,
        string reason,
        Guid? customerUserId = null)
    {
        if (customerOrderId == Guid.Empty)
        {
            throw new DomainException("Customer order id is required.");
        }

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new DomainException("Customer email is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Return reason is required.");
        }

        return new ReturnRequest
        {
            Id = Guid.NewGuid(),
            CustomerOrderId = customerOrderId,
            CustomerUserId = customerUserId,
            CustomerEmail = customerEmail.Trim().ToLowerInvariant(),
            Reason = reason.Trim(),
            Status = ReturnRequestStatus.Requested,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Approve(string? adminNotes = null)
    {
        if (Status != ReturnRequestStatus.Requested)
        {
            throw new DomainException("Only requested returns can be approved.");
        }

        Status = ReturnRequestStatus.Approved;
        AdminNotes = adminNotes?.Trim();
        ResolvedAtUtc = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != ReturnRequestStatus.Requested)
        {
            throw new DomainException("Only requested returns can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Rejection reason is required.");
        }

        Status = ReturnRequestStatus.Rejected;
        AdminNotes = reason.Trim();
        ResolvedAtUtc = DateTime.UtcNow;
    }

    public void MarkReceived(string? adminNotes = null)
    {
        if (Status != ReturnRequestStatus.Approved)
        {
            throw new DomainException("Only approved returns can be marked as received.");
        }

        Status = ReturnRequestStatus.Received;
        if (!string.IsNullOrWhiteSpace(adminNotes))
        {
            AdminNotes = adminNotes.Trim();
        }
    }

    public void MarkRefunded()
    {
        if (Status != ReturnRequestStatus.Approved && Status != ReturnRequestStatus.Received)
        {
            throw new DomainException("Return cannot be refunded in the current status.");
        }

        Status = ReturnRequestStatus.Refunded;
        ResolvedAtUtc = DateTime.UtcNow;
    }
}
