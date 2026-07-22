using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class OutboxStore : IOutboxStore, IOutboxReader
{
    private readonly BfaDbContext _dbContext;

    public OutboxStore(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnqueueAsync(
        string type,
        string payload,
        CancellationToken cancellationToken = default)
    {
        var message = OutboxMessage.Create(type, payload);
        await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PendingOutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.ProcessedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .Take(batchSize)
            .Select(message => new PendingOutboxMessage(
                message.Id,
                message.Type,
                message.Payload,
                message.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.OutboxMessages
            .FirstOrDefaultAsync(item => item.Id == messageId, cancellationToken);

        if (message is null)
        {
            return;
        }

        message.MarkProcessed();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class AuditLogger : IAuditLogger
{
    private readonly BfaDbContext _dbContext;

    public AuditLogger(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(
        string actorType,
        Guid? actorId,
        string action,
        string entityType,
        Guid? entityId,
        string? detailsJson = null,
        CancellationToken cancellationToken = default)
    {
        var entry = AuditEntry.Create(
            actorType,
            actorId,
            action,
            entityType,
            entityId,
            detailsJson);

        await _dbContext.AuditEntries.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly BfaDbContext _dbContext;

    public AuditLogRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuditEntryDto>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 500);

        return await _dbContext.AuditEntries
            .AsNoTracking()
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Take(limit)
            .Select(entry => new AuditEntryDto(
                entry.Id,
                entry.ActorType,
                entry.ActorId,
                entry.Action,
                entry.EntityType,
                entry.EntityId,
                entry.DetailsJson,
                entry.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }
}
