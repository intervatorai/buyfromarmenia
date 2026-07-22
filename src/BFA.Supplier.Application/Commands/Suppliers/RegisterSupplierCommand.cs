using BFA.BuildingBlocks.Application;
using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using MediatR;
using BFA.Modules.Identity.Domain.Auth;

namespace BFA.Supplier.Application.Commands.Suppliers;

public record RegisterSupplierCommand(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string Password,
    string? TaxNumber = null,
    string? RegistrationNumber = null,
    string? LegalCountryCode = null,
    string? LegalCity = null,
    string? LegalLine1 = null,
    string? LegalLine2 = null,
    string? LegalPostalCode = null,
    string? LegalRegion = null) : IRequest<RegisterSupplierResult>;

public record RegisterSupplierResult(
    Guid SupplierId,
    string AccessToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string FullName,
    string TradingName,
    string Role);

public class RegisterSupplierCommandHandler : IRequestHandler<RegisterSupplierCommand, RegisterSupplierResult>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditLogger _auditLogger;
    private readonly IOutboxStore _outboxStore;

    public RegisterSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IAuditLogger auditLogger,
        IOutboxStore outboxStore)
    {
        _supplierRepository = supplierRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _auditLogger = auditLogger;
        _outboxStore = outboxStore;
    }

    public async Task<RegisterSupplierResult> Handle(
        RegisterSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var existingSupplier = await _supplierRepository.GetByContactEmailAsync(
            request.Email,
            cancellationToken);
        if (existingSupplier is not null)
        {
            throw new InvalidOperationException("A supplier with this email already exists.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var contact = new ContactInformation(request.ContactPerson, request.Email, request.Phone);
        var legalAddress = TryCreateAddress(
            request.LegalCountryCode,
            request.LegalCity,
            request.LegalLine1,
            request.LegalLine2,
            request.LegalPostalCode,
            request.LegalRegion);

        var supplier = Modules.Suppliers.Domain.Aggregates.Supplier.Register(
            request.LegalName,
            request.TradingName,
            contact,
            legalAddress,
            request.TaxNumber,
            request.RegistrationNumber);

        var user = User.RegisterSupplier(
            request.Email,
            _passwordHasher.Hash(request.Password));
        supplier.LinkOwnerToUser(user.Id);

        await _userRepository.AddAsync(user, cancellationToken);
        await _supplierRepository.AddAsync(supplier, cancellationToken);

        await _auditLogger.WriteAsync(
            "Supplier",
            user.Id,
            "SupplierRegistered",
            "Supplier",
            supplier.Id,
            cancellationToken: cancellationToken);

        await _outboxStore.EnqueueAsync(
            IntegrationEventTypes.SupplierRegistered,
            $"{{\"supplierId\":\"{supplier.Id}\",\"email\":\"{user.Email}\"}}",
            cancellationToken);

        var token = _jwtTokenService.GenerateSupplierToken(
            user,
            supplier.Id,
            supplier.TradingName,
            Modules.Suppliers.Domain.Enums.SupplierMemberRole.Owner.ToString());

        return new RegisterSupplierResult(
            supplier.Id,
            token.AccessToken,
            token.ExpiresAt,
            user.Id,
            user.Email,
            request.ContactPerson.Trim(),
            supplier.TradingName,
            Modules.Suppliers.Domain.Enums.SupplierMemberRole.Owner.ToString());
    }

    private static Address? TryCreateAddress(
        string? countryCode,
        string? city,
        string? line1,
        string? line2,
        string? postalCode,
        string? region)
    {
        if (string.IsNullOrWhiteSpace(countryCode)
            || string.IsNullOrWhiteSpace(city)
            || string.IsNullOrWhiteSpace(line1))
        {
            return null;
        }

        return new Address(countryCode, city, line1, line2, postalCode, region);
    }
}
