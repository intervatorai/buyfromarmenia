using BFA.Modules.Returns.Domain.Aggregates;
using BFA.Modules.Returns.Domain.Enums;

namespace BFA.Modules.Returns.Domain.Repositories;

public interface IReturnRequestRepository
{
    Task<ReturnRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReturnRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReturnRequest?> GetOpenByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReturnRequest>> GetByStatusAsync(
        ReturnRequestStatus? status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReturnRequest>> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);

    Task AddAsync(ReturnRequest returnRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReturnRequest returnRequest, CancellationToken cancellationToken = default);
}
