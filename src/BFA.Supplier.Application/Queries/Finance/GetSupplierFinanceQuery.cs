using BFA.Modules.Settlements.Domain.Enums;
using BFA.Modules.Settlements.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Finance;

public record GetSupplierFinanceQuery(Guid SupplierId) : IRequest<SupplierFinanceDto>;

public record SupplierFinanceDto(
    decimal PendingBalance,
    decimal EligibleBalance,
    decimal PaidTotal,
    string Currency,
    IReadOnlyList<SupplierSettlementDto> Settlements,
    IReadOnlyList<SupplierPayoutDto> Payouts);

public record SupplierSettlementDto(
    Guid Id,
    Guid SupplierOrderId,
    decimal GrossAmount,
    decimal PlatformFee,
    decimal NetAmount,
    string Currency,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? EligibleAtUtc);

public record SupplierPayoutDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTime ScheduledForUtc,
    DateTime? CompletedAtUtc);

public sealed class GetSupplierFinanceQueryHandler
    : IRequestHandler<GetSupplierFinanceQuery, SupplierFinanceDto>
{
    private readonly ISupplierSettlementRepository _settlementRepository;
    private readonly IPayoutRepository _payoutRepository;

    public GetSupplierFinanceQueryHandler(
        ISupplierSettlementRepository settlementRepository,
        IPayoutRepository payoutRepository)
    {
        _settlementRepository = settlementRepository;
        _payoutRepository = payoutRepository;
    }

    public async Task<SupplierFinanceDto> Handle(
        GetSupplierFinanceQuery request,
        CancellationToken cancellationToken)
    {
        var settlements = await _settlementRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);
        var payouts = await _payoutRepository.GetBySupplierIdAsync(
            request.SupplierId,
            cancellationToken);

        var currency = settlements.FirstOrDefault()?.Currency ?? "USD";

        return new SupplierFinanceDto(
            settlements.Where(s => s.Status == SettlementStatus.Pending).Sum(s => s.NetAmount),
            settlements.Where(s => s.Status == SettlementStatus.Eligible).Sum(s => s.NetAmount),
            settlements.Where(s => s.Status == SettlementStatus.Paid).Sum(s => s.NetAmount),
            currency,
            settlements.Select(s => new SupplierSettlementDto(
                s.Id,
                s.SupplierOrderId,
                s.GrossAmount,
                s.PlatformFee,
                s.NetAmount,
                s.Currency,
                s.Status.ToString(),
                s.CreatedAtUtc,
                s.EligibleAtUtc)).ToList(),
            payouts.Select(p => new SupplierPayoutDto(
                p.Id,
                p.Amount,
                p.Currency,
                p.Status.ToString(),
                p.ScheduledForUtc,
                p.CompletedAtUtc)).ToList());
    }
}
