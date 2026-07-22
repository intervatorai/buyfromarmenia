namespace BFA.Modules.Suppliers.Domain.Events;

public sealed record SupplierRegisteredDomainEvent(Guid SupplierId) : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record SupplierApplicationSubmittedDomainEvent(Guid SupplierId) : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record SupplierApprovedDomainEvent(Guid SupplierId) : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record SupplierRejectedDomainEvent(Guid SupplierId, string Reason) : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record SupplierSuspendedDomainEvent(Guid SupplierId, string Reason) : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record SupplierActivatedDomainEvent(Guid SupplierId) : BFA.BuildingBlocks.Domain.DomainEvent;

public sealed record SupplierBankAccountChangedDomainEvent(Guid SupplierId) : BFA.BuildingBlocks.Domain.DomainEvent;
