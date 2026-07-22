namespace BFA.BuildingBlocks.Application;

public interface IOutboxStore
{
    Task EnqueueAsync(string type, string payload, CancellationToken cancellationToken = default);
}

public record PendingOutboxMessage(
    Guid Id,
    string Type,
    string Payload,
    DateTime OccurredAtUtc);

public interface IOutboxReader
{
    Task<IReadOnlyList<PendingOutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
}

public interface IOutboxProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}

public interface IAuditLogger
{
    Task WriteAsync(
        string actorType,
        Guid? actorId,
        string action,
        string entityType,
        Guid? entityId,
        string? detailsJson = null,
        CancellationToken cancellationToken = default);
}

public record AuditEntryDto(
    Guid Id,
    string ActorType,
    Guid? ActorId,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? DetailsJson,
    DateTime OccurredAtUtc);

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditEntryDto>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default);
}
