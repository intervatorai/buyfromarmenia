using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.Enums;
using BFA.Modules.Suppliers.Domain.Events;
using BFA.Modules.Suppliers.Domain.ValueObjects;

namespace BFA.Modules.Suppliers.Domain.Aggregates;

public sealed class Supplier : AggregateRoot
{
    public string LegalName { get; private set; } = string.Empty;
    public string TradingName { get; private set; } = string.Empty;
    public string? TaxNumber { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public SupplierStatus Status { get; private set; } = SupplierStatus.Draft;
    public ContactInformation Contact { get; private set; } = null!;
    public Address? LegalAddress { get; private set; }
    public Address? WarehouseAddress { get; private set; }
    public Guid? CommissionPlanId { get; private set; }
    public TimeSpan DefaultPreparationTime { get; private set; } = TimeSpan.FromDays(2);
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<SupplierMember> _members = [];
    private readonly List<SupplierDocument> _documents = [];
    private readonly List<SupplierBankAccount> _bankAccounts = [];

    public IReadOnlyCollection<SupplierMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<SupplierDocument> Documents => _documents.AsReadOnly();
    public IReadOnlyCollection<SupplierBankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    private Supplier()
    {
    }

    public static Supplier Register(
        string legalName,
        string tradingName,
        ContactInformation contact,
        Address? legalAddress = null,
        string? taxNumber = null,
        string? registrationNumber = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            throw new DomainException("Legal name is required.");
        }

        if (string.IsNullOrWhiteSpace(tradingName))
        {
            throw new DomainException("Trading name is required.");
        }

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            LegalName = legalName.Trim(),
            TradingName = tradingName.Trim(),
            TaxNumber = taxNumber?.Trim(),
            RegistrationNumber = registrationNumber?.Trim(),
            Contact = contact,
            LegalAddress = legalAddress,
            Status = SupplierStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        supplier._members.Add(new SupplierMember(
            supplier.Id,
            contact.Email,
            contact.ContactPerson,
            SupplierMemberRole.Owner));

        supplier.RaiseDomainEvent(new SupplierRegisteredDomainEvent(supplier.Id));
        return supplier;
    }

    public void LinkOwnerToUser(Guid userId)
    {
        var owner = _members.FirstOrDefault(
            member => member.Role == SupplierMemberRole.Owner && member.IsActive);

        if (owner is null)
        {
            throw new DomainException("Owner member not found.");
        }

        if (owner.UserId.HasValue)
        {
            throw new DomainException("Owner account is already linked.");
        }

        owner.AssignUser(userId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(
        string legalName,
        string tradingName,
        ContactInformation contact,
        Address? legalAddress,
        string? taxNumber,
        string? registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(legalName) || string.IsNullOrWhiteSpace(tradingName))
        {
            throw new DomainException("Legal and trading names are required.");
        }

        LegalName = legalName.Trim();
        TradingName = tradingName.Trim();
        Contact = contact;
        LegalAddress = legalAddress;
        TaxNumber = taxNumber?.Trim();
        RegistrationNumber = registrationNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetWarehouseAddress(Address? warehouseAddress)
    {
        WarehouseAddress = warehouseAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCommissionPlan(Guid? commissionPlanId)
    {
        CommissionPlanId = commissionPlanId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultPreparationTime(TimeSpan preparationTime)
    {
        if (preparationTime <= TimeSpan.Zero)
        {
            throw new DomainException("Preparation time must be positive.");
        }

        DefaultPreparationTime = preparationTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public SupplierMember AddMember(string email, string fullName, SupplierMemberRole role, Guid? userId = null)
    {
        if (_members.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"Member with email '{email}' already exists.");
        }

        var member = new SupplierMember(Id, email, fullName, role, userId);
        _members.Add(member);
        UpdatedAt = DateTime.UtcNow;
        return member;
    }

    public SupplierDocument AddDocument(SupplierDocumentType documentType, string fileName, string fileUrl)
    {
        var document = new SupplierDocument(Id, documentType, fileName, fileUrl);
        _documents.Add(document);
        UpdatedAt = DateTime.UtcNow;
        return document;
    }

    public SupplierBankAccount AddBankAccount(BankAccountDetails details, bool isPrimary = false)
    {
        if (isPrimary)
        {
            foreach (var account in _bankAccounts.Where(a => a.IsPrimary))
            {
                account.SetPrimary(false);
            }
        }

        var bankAccount = new SupplierBankAccount(Id, details, isPrimary);
        _bankAccounts.Add(bankAccount);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierBankAccountChangedDomainEvent(Id));
        return bankAccount;
    }

    public void UpdateBankAccount(
        Guid bankAccountId,
        BankAccountDetails details,
        bool isPrimary)
    {
        var bankAccount = _bankAccounts.FirstOrDefault(account => account.Id == bankAccountId)
            ?? throw new DomainException("Bank account not found.");

        if (isPrimary)
        {
            foreach (var account in _bankAccounts.Where(a => a.IsPrimary && a.Id != bankAccountId))
            {
                account.SetPrimary(false);
            }
        }

        bankAccount.Update(details);
        bankAccount.SetPrimary(isPrimary);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierBankAccountChangedDomainEvent(Id));
    }

    public void RemoveBankAccount(Guid bankAccountId)
    {
        var bankAccount = _bankAccounts.FirstOrDefault(account => account.Id == bankAccountId)
            ?? throw new DomainException("Bank account not found.");

        _bankAccounts.Remove(bankAccount);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierBankAccountChangedDomainEvent(Id));
    }

    public void UpdateDocument(
        Guid documentId,
        SupplierDocumentType documentType,
        string fileName,
        string fileUrl)
    {
        var document = _documents.FirstOrDefault(item => item.Id == documentId)
            ?? throw new DomainException("Document not found.");

        document.Update(documentType, fileName, fileUrl);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDocument(Guid documentId)
    {
        var document = _documents.FirstOrDefault(item => item.Id == documentId)
            ?? throw new DomainException("Document not found.");

        _documents.Remove(document);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SubmitApplication()
    {
        if (Status is not (SupplierStatus.Draft or SupplierStatus.ChangesRequested))
        {
            throw new DomainException("Supplier application cannot be submitted in the current status.");
        }

        Status = SupplierStatus.ApplicationSubmitted;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierApplicationSubmittedDomainEvent(Id));
    }

    public void MarkUnderReview()
    {
        if (Status != SupplierStatus.ApplicationSubmitted)
        {
            throw new DomainException("Only submitted applications can be moved to review.");
        }

        Status = SupplierStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status is not (SupplierStatus.UnderReview or SupplierStatus.Approved))
        {
            throw new DomainException("Supplier cannot be approved in the current status.");
        }

        Status = SupplierStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierApprovedDomainEvent(Id));
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Rejection reason is required.");
        }

        Status = SupplierStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierRejectedDomainEvent(Id, reason));
    }

    public void RequestChanges(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Change request reason is required.");
        }

        Status = SupplierStatus.ChangesRequested;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend(string reason)
    {
        if (Status != SupplierStatus.Active)
        {
            throw new DomainException("Only active suppliers can be suspended.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Suspension reason is required.");
        }

        Status = SupplierStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierSuspendedDomainEvent(Id, reason));
    }

    public void Activate()
    {
        if (Status is not (SupplierStatus.Approved or SupplierStatus.Suspended))
        {
            throw new DomainException("Supplier cannot be activated in the current status.");
        }

        Status = SupplierStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SupplierActivatedDomainEvent(Id));
    }

    public void Close()
    {
        Status = SupplierStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void LoadMembers(IEnumerable<SupplierMember> members)
    {
        _members.Clear();
        _members.AddRange(members);
    }

    internal void LoadDocuments(IEnumerable<SupplierDocument> documents)
    {
        _documents.Clear();
        _documents.AddRange(documents);
    }

    internal void LoadBankAccounts(IEnumerable<SupplierBankAccount> bankAccounts)
    {
        _bankAccounts.Clear();
        _bankAccounts.AddRange(bankAccounts);
    }
}
