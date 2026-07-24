using BFA.BuildingBlocks.Application;
using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using MediatR;

namespace BFA.Admin.Application.Commands.Suppliers;

public record AddSupplierBankAccountCommand(
    Guid SupplierId,
    string BankName,
    string AccountHolder,
    string Iban,
    string Currency,
    string? Swift = null,
    bool IsPrimary = true) : IRequest<bool>;

public sealed class AddSupplierBankAccountCommandHandler
    : IRequestHandler<AddSupplierBankAccountCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAuditLogger _auditLogger;

    public AddSupplierBankAccountCommandHandler(
        ISupplierRepository supplierRepository,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        AddSupplierBankAccountCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return false;
        }

        var details = new BankAccountDetails(
            request.BankName,
            request.AccountHolder,
            request.Iban,
            request.Currency,
            request.Swift);

        supplier.AddBankAccount(details, request.IsPrimary);
        await _supplierRepository.UpdateAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SupplierBankAccountAdded",
            "Supplier",
            supplier.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record SetSupplierPasswordCommand(
    Guid SupplierId,
    string NewPassword) : IRequest<SetSupplierPasswordResult>;

public record SetSupplierPasswordResult(bool Success, string? Error = null);

public sealed class SetSupplierPasswordCommandHandler
    : IRequestHandler<SetSupplierPasswordCommand, SetSupplierPasswordResult>
{
    private const int MinPasswordLength = 6;

    private readonly ISupplierRepository _supplierRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public SetSupplierPasswordCommandHandler(
        ISupplierRepository supplierRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<SetSupplierPasswordResult> Handle(
        SetSupplierPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword)
            || request.NewPassword.Length < MinPasswordLength)
        {
            return new SetSupplierPasswordResult(
                false,
                $"Password must be at least {MinPasswordLength} characters.");
        }

        var supplier = await _supplierRepository.GetByIdForUpdateAsync(
            request.SupplierId,
            cancellationToken);
        if (supplier is null)
        {
            return new SetSupplierPasswordResult(false, "Supplier not found.");
        }

        var owner = supplier.Members.FirstOrDefault(member =>
            member.IsActive && member.Role == Modules.Suppliers.Domain.Enums.SupplierMemberRole.Owner);
        if (owner is null)
        {
            return new SetSupplierPasswordResult(false, "Supplier owner account not found.");
        }

        User? user = null;
        if (owner.UserId.HasValue)
        {
            user = await _userRepository.GetByIdForUpdateAsync(owner.UserId.Value, cancellationToken);
        }
        else
        {
            var existing = await _userRepository.GetByEmailAsync(owner.Email, cancellationToken);
            if (existing is not null)
            {
                if (existing.Type != UserType.Supplier)
                {
                    return new SetSupplierPasswordResult(
                        false,
                        "Email is already used by a non-supplier account.");
                }

                user = await _userRepository.GetByIdForUpdateAsync(existing.Id, cancellationToken);
                if (user is null)
                {
                    return new SetSupplierPasswordResult(false, "Supplier login user not found.");
                }

                supplier.LinkOwnerToUser(user.Id);
                await _supplierRepository.UpdateAsync(supplier, cancellationToken);
            }
            else
            {
                user = User.RegisterSupplier(owner.Email, _passwordHasher.Hash(request.NewPassword));
                await _userRepository.AddAsync(user, cancellationToken);
                supplier.LinkOwnerToUser(user.Id);
                await _supplierRepository.UpdateAsync(supplier, cancellationToken);

                await _auditLogger.WriteAsync(
                    "Admin",
                    null,
                    "SupplierPortalUserCreated",
                    "Supplier",
                    supplier.Id,
                    cancellationToken: cancellationToken);

                return new SetSupplierPasswordResult(true);
            }
        }

        if (user is null || user.Type != UserType.Supplier)
        {
            return new SetSupplierPasswordResult(false, "Supplier login user not found.");
        }

        user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        await _userRepository.UpdateAsync(user, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SupplierPasswordSet",
            "Supplier",
            supplier.Id,
            cancellationToken: cancellationToken);

        return new SetSupplierPasswordResult(true);
    }
}
