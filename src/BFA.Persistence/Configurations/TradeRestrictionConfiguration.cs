using BFA.Modules.Compliance.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class TradeRestrictionConfiguration : IEntityTypeConfiguration<TradeRestriction>
{
    public void Configure(EntityTypeBuilder<TradeRestriction> builder)
    {
        builder.ToTable("trade_restrictions", "compliance");
        builder.HasKey(restriction => restriction.Id);
        builder.HasIndex(restriction => restriction.DestinationCountryCode);
        builder.HasIndex(restriction => restriction.IsActive);
        builder.Property(restriction => restriction.DestinationCountryCode).HasMaxLength(2).IsRequired();
        builder.Property(restriction => restriction.Reason).HasMaxLength(500).IsRequired();
        builder.Ignore(restriction => restriction.DomainEvents);
    }
}
