using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Suppliers;

public record GetSuppliersQuery(string? Status = null) : IRequest<IReadOnlyList<SupplierListItemDto>>;

public record SupplierListItemDto(
    Guid Id,
    string LegalName,
    string TradingName,
    string Status,
    string ContactPerson,
    string Email,
    string Phone,
    int BankAccountsCount,
    int DocumentsCount,
    DateTime CreatedAt);

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, IReadOnlyList<SupplierListItemDto>>
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSuppliersQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<IReadOnlyList<SupplierListItemDto>> Handle(
        GetSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = string.IsNullOrWhiteSpace(request.Status)
            ? await _supplierRepository.GetAllAsync(cancellationToken)
            : await _supplierRepository.GetByStatusAsync(
                Enum.Parse<BFA.Modules.Suppliers.Domain.Enums.SupplierStatus>(request.Status, true),
                cancellationToken);

        return suppliers.Select(s => new SupplierListItemDto(
            s.Id,
            s.LegalName,
            s.TradingName,
            s.Status.ToString(),
            s.Contact.ContactPerson,
            s.Contact.Email,
            s.Contact.Phone,
            s.BankAccounts.Count,
            s.Documents.Count,
            s.CreatedAt)).ToList();
    }
}
