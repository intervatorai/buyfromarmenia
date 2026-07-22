using BFA.Modules.Warehouse.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class ConsolidationConfiguration : IEntityTypeConfiguration<Consolidation>
{
    public void Configure(EntityTypeBuilder<Consolidation> builder)
    {
        builder.ToTable("consolidations", "warehouse");
        builder.HasKey(consolidation => consolidation.Id);
        builder.HasIndex(consolidation => consolidation.ReferenceNumber).IsUnique();
        builder.HasIndex(consolidation => consolidation.CustomerOrderId);
        builder.Property(consolidation => consolidation.ReferenceNumber).HasMaxLength(32).IsRequired();
        builder.Property(consolidation => consolidation.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(consolidation => consolidation.CreatedAtUtc).IsRequired();
        builder.Property(consolidation => consolidation.UpdatedAtUtc).IsRequired();

        builder.HasMany<ConsolidationItem>("_items")
            .WithOne()
            .HasForeignKey(item => item.ConsolidationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Package>("_packages")
            .WithOne()
            .HasForeignKey(package => package.ConsolidationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_packages").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(consolidation => consolidation.Items);
        builder.Ignore(consolidation => consolidation.Packages);
        builder.Ignore(consolidation => consolidation.InboundShipmentIds);
        builder.Ignore(consolidation => consolidation.DomainEvents);
    }
}

public sealed class ConsolidationItemConfiguration : IEntityTypeConfiguration<ConsolidationItem>
{
    public void Configure(EntityTypeBuilder<ConsolidationItem> builder)
    {
        builder.ToTable("consolidation_items", "warehouse");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.InboundShipmentId).IsUnique();
    }
}

public sealed class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("packages", "warehouse");
        builder.HasKey(package => package.Id);
        builder.Property(package => package.Label).HasMaxLength(32).IsRequired();
        builder.Property(package => package.WeightKg).HasPrecision(10, 3);
        builder.Property(package => package.Notes).HasMaxLength(500);
        builder.Property(package => package.CreatedAtUtc).IsRequired();
    }
}
