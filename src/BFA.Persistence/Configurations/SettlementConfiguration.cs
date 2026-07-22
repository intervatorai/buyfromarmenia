using BFA.Modules.Settlements.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class SupplierSettlementConfiguration : IEntityTypeConfiguration<SupplierSettlement>
{
    public void Configure(EntityTypeBuilder<SupplierSettlement> builder)
    {
        builder.ToTable("supplier_settlements", "settlements");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.SupplierOrderId).IsUnique();
        builder.HasIndex(s => s.SupplierId);
        builder.Property(s => s.GrossAmount).HasPrecision(18, 2);
        builder.Property(s => s.PlatformFee).HasPrecision(18, 2);
        builder.Property(s => s.NetAmount).HasPrecision(18, 2);
        builder.Property(s => s.Currency).HasMaxLength(3).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(24);
        builder.Ignore(s => s.DomainEvents);
    }
}

public sealed class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("payouts", "settlements");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.SupplierId);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(24);
        builder.Ignore(p => p.DomainEvents);
    }
}
