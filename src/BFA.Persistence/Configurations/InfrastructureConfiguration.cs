using BFA.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "infrastructure");
        builder.HasKey(message => message.Id);
        builder.HasIndex(message => message.ProcessedAtUtc);
        builder.Property(message => message.Type).HasMaxLength(200).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
    }
}

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries", "infrastructure");
        builder.HasKey(entry => entry.Id);
        builder.HasIndex(entry => entry.OccurredAtUtc);
        builder.HasIndex(entry => new { entry.EntityType, entry.EntityId });
        builder.Property(entry => entry.ActorType).HasMaxLength(32).IsRequired();
        builder.Property(entry => entry.Action).HasMaxLength(120).IsRequired();
        builder.Property(entry => entry.EntityType).HasMaxLength(120).IsRequired();
        builder.Property(entry => entry.DetailsJson).HasColumnType("jsonb");
    }
}
