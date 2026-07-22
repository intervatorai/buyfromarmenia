namespace BFA.BuildingBlocks.Domain;

public sealed class AuditEntry : Entity
{
    public string ActorType { get; private set; } = string.Empty;
    public Guid? ActorId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? DetailsJson { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private AuditEntry()
    {
    }

    public static AuditEntry Create(
        string actorType,
        Guid? actorId,
        string action,
        string entityType,
        Guid? entityId,
        string? detailsJson = null)
    {
        if (string.IsNullOrWhiteSpace(actorType))
        {
            throw new DomainException("Actor type is required.");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new DomainException("Audit action is required.");
        }

        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new DomainException("Entity type is required.");
        }

        return new AuditEntry
        {
            Id = Guid.NewGuid(),
            ActorType = actorType.Trim(),
            ActorId = actorId,
            Action = action.Trim(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            DetailsJson = detailsJson,
            OccurredAtUtc = DateTime.UtcNow
        };
    }
}
