using BFA.Modules.Compliance.Domain.Aggregates;

namespace BFA.Modules.Compliance.Domain.Repositories;

public interface ITradeRestrictionRepository
{
    Task<IReadOnlyList<TradeRestriction>> GetActiveForCountryAsync(
        string destinationCountryCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TradeRestriction>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<TradeRestriction?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(TradeRestriction restriction, CancellationToken cancellationToken = default);
    Task UpdateAsync(TradeRestriction restriction, CancellationToken cancellationToken = default);
}
