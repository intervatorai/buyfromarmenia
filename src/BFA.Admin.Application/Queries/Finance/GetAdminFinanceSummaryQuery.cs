using BFA.Modules.Settlements.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Finance;

public record GetAdminFinanceSummaryQuery() : IRequest<AdminFinanceSummaryDto>;

public record AdminFinanceSummaryDto(
    int ActiveSuppliersCount,
    decimal TotalPendingSettlements,
    decimal TotalEligibleSettlements,
    decimal TotalPaidSettlements,
    string Currency);

public sealed class GetAdminFinanceSummaryQueryHandler
    : IRequestHandler<GetAdminFinanceSummaryQuery, AdminFinanceSummaryDto>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ISupplierSettlementRepository _settlementRepository;

    public GetAdminFinanceSummaryQueryHandler(
        ISupplierRepository supplierRepository,
        ISupplierSettlementRepository settlementRepository)
    {
        _supplierRepository = supplierRepository;
        _settlementRepository = settlementRepository;
    }

    public async Task<AdminFinanceSummaryDto> Handle(
        GetAdminFinanceSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = await _supplierRepository.GetAllAsync(cancellationToken);
        var allSettlements = await _settlementRepository.GetAllAsync(cancellationToken);
        var currency = allSettlements.FirstOrDefault()?.Currency ?? "USD";

        return new AdminFinanceSummaryDto(
            suppliers.Count,
            allSettlements.Where(s => s.Status == BFA.Modules.Settlements.Domain.Enums.SettlementStatus.Pending)
                .Sum(s => s.NetAmount),
            allSettlements.Where(s => s.Status == BFA.Modules.Settlements.Domain.Enums.SettlementStatus.Eligible)
                .Sum(s => s.NetAmount),
            allSettlements.Where(s => s.Status == BFA.Modules.Settlements.Domain.Enums.SettlementStatus.Paid)
                .Sum(s => s.NetAmount),
            currency);
    }
}
