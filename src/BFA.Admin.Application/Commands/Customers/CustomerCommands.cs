using BFA.BuildingBlocks.Application;
using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Customers;

public record CreateCustomerCommand(
    string Email,
    string Password,
    string FullName,
    string? Phone) : IRequest<Guid?>;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid?>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public CreateCustomerCommandHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository,
        IPasswordHasher passwordHasher,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<Guid?> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return null;
        }

        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            return null;
        }

        var user = User.RegisterCustomer(
            request.Email,
            _passwordHasher.Hash(request.Password));
        var profile = CustomerProfile.Create(user.Id, request.FullName, request.Phone);

        await _userRepository.AddAsync(user, cancellationToken);
        await _profileRepository.AddAsync(profile, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CustomerCreated",
            "Customer",
            user.Id,
            cancellationToken: cancellationToken);

        return user.Id;
    }
}

public record UpdateCustomerCommand(
    Guid CustomerId,
    string FullName,
    string? Phone,
    string? NewPassword = null) : IRequest<bool>;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public UpdateCustomerCommandHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository,
        IPasswordHasher passwordHasher,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(request.CustomerId, cancellationToken);
        if (user is null || user.Type != UserType.Customer)
        {
            return false;
        }

        var profile = await _profileRepository.GetByUserIdForUpdateAsync(
            user.Id,
            cancellationToken);
        if (profile is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return false;
        }

        profile.Update(request.FullName, request.Phone);

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (request.NewPassword.Length < 6)
            {
                return false;
            }

            user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        await _profileRepository.UpdateAsync(profile, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CustomerUpdated",
            "Customer",
            user.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record SuspendCustomerCommand(Guid CustomerId) : IRequest<bool>;

public sealed class SuspendCustomerCommandHandler : IRequestHandler<SuspendCustomerCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogger _auditLogger;

    public SuspendCustomerCommandHandler(
        IUserRepository userRepository,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(SuspendCustomerCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(request.CustomerId, cancellationToken);
        if (user is null || user.Type != UserType.Customer)
        {
            return false;
        }

        user.Suspend();
        await _userRepository.UpdateAsync(user, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CustomerSuspended",
            "Customer",
            user.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record ActivateCustomerCommand(Guid CustomerId) : IRequest<bool>;

public sealed class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogger _auditLogger;

    public ActivateCustomerCommandHandler(
        IUserRepository userRepository,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdForUpdateAsync(request.CustomerId, cancellationToken);
        if (user is null || user.Type != UserType.Customer)
        {
            return false;
        }

        user.Activate();
        await _userRepository.UpdateAsync(user, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CustomerActivated",
            "Customer",
            user.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
