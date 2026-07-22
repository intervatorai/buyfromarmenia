namespace BFA.BuildingBlocks.Domain;

public sealed class OutboxMessage : Entity
{
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    private OutboxMessage()
    {
    }

    public static OutboxMessage Create(string type, string payload)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new DomainException("Outbox message type is required.");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new DomainException("Outbox message payload is required.");
        }

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type.Trim(),
            Payload = payload,
            OccurredAtUtc = DateTime.UtcNow
        };
    }

    public void MarkProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
