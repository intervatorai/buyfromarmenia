namespace BFA.Modules.Catalog.Domain.Events;

public sealed record CategoryCreatedDomainEvent(Guid CategoryId, Guid? ParentCategoryId)
    : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record ProductCreatedDomainEvent(Guid ProductId, Guid SupplierId)
    : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record ProductSubmittedForReviewDomainEvent(Guid ProductId, Guid SupplierId)
    : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record ProductApprovedDomainEvent(Guid ProductId, Guid SupplierId)
    : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record ProductRejectedDomainEvent(Guid ProductId, Guid SupplierId, string Reason)
    : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record ProductPublishedDomainEvent(Guid ProductId, Guid SupplierId)
    : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record VariantAddedDomainEvent(Guid ProductId, Guid VariantId, string SupplierSku)
    : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record ProductShippingProfileChangedDomainEvent(Guid ProductId, Guid SupplierId)
    : BFA.BuildingBlocks.Domain.DomainEvent;
