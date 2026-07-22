using BFA.Modules.Shipping.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments", "shipping");
        builder.HasKey(shipment => shipment.Id);
        builder.HasIndex(shipment => shipment.ReferenceNumber).IsUnique();
        builder.HasIndex(shipment => shipment.TrackingNumber).IsUnique();
        builder.HasIndex(shipment => shipment.ConsolidationId).IsUnique();
        builder.HasIndex(shipment => shipment.CustomerOrderId);
        builder.Property(shipment => shipment.ReferenceNumber).HasMaxLength(32).IsRequired();
        builder.Property(shipment => shipment.Carrier).HasConversion<string>().HasMaxLength(24);
        builder.Property(shipment => shipment.TrackingNumber).HasMaxLength(64).IsRequired();
        builder.Property(shipment => shipment.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(shipment => shipment.CreatedAtUtc).IsRequired();
        builder.Property(shipment => shipment.UpdatedAtUtc).IsRequired();

        builder.OwnsOne(shipment => shipment.Customs, customs =>
        {
            customs.Property(c => c.Description).HasColumnName("customs_description").HasMaxLength(500);
            customs.Property(c => c.HsCode).HasColumnName("customs_hs_code").HasMaxLength(32);
            customs.Property(c => c.DeclaredValue).HasColumnName("customs_declared_value").HasPrecision(18, 2);
            customs.Property(c => c.Currency).HasColumnName("customs_currency").HasMaxLength(3);
        });

        builder.Ignore(shipment => shipment.DomainEvents);
    }
}
