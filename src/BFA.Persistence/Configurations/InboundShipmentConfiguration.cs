using BFA.Modules.Warehouse.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class InboundShipmentConfiguration : IEntityTypeConfiguration<InboundShipment>
{
    public void Configure(EntityTypeBuilder<InboundShipment> builder)
    {
        builder.ToTable("inbound_shipments", "warehouse");
        builder.HasKey(shipment => shipment.Id);
        builder.HasIndex(shipment => shipment.ReferenceNumber).IsUnique();
        builder.HasIndex(shipment => shipment.SupplierOrderId).IsUnique();
        builder.HasIndex(shipment => shipment.SupplierId);
        builder.Property(shipment => shipment.ReferenceNumber).HasMaxLength(32).IsRequired();
        builder.Property(shipment => shipment.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(shipment => shipment.ConsolidationId);
        builder.HasIndex(shipment => shipment.ConsolidationId);
        builder.Property(shipment => shipment.CreatedAtUtc).IsRequired();
        builder.Property(shipment => shipment.UpdatedAtUtc).IsRequired();

        builder.OwnsOne(shipment => shipment.Receipt, receipt =>
        {
            receipt.Property(r => r.ScanReference).HasColumnName("receipt_scan_reference").HasMaxLength(128);
            receipt.Property(r => r.WeightKg).HasColumnName("receipt_weight_kg").HasPrecision(10, 3);
            receipt.Property(r => r.InspectionNotes).HasColumnName("receipt_inspection_notes").HasMaxLength(2000);
            receipt.Property(r => r.PhotoUrl).HasColumnName("receipt_photo_url").HasMaxLength(2048);
            receipt.Property(r => r.ReceivedBy).HasColumnName("receipt_received_by").HasMaxLength(200);
            receipt.Property(r => r.ReceivedAtUtc).HasColumnName("receipt_received_at_utc");
        });

        builder.Ignore(shipment => shipment.DomainEvents);
    }
}
