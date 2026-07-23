using BFA.Modules.Inventory.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items", "inventory");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.SupplierId).IsRequired();
        builder.Property(item => item.ProductId).IsRequired();
        builder.Property(item => item.ProductVariantId).IsRequired();
        builder.Property(item => item.OnHand).IsRequired();
        builder.Property(item => item.Reserved).IsRequired();
        builder.Property(item => item.LowStockThreshold).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();
        builder.Property(item => item.UpdatedAtUtc).IsRequired();
        builder.Property(item => item.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(item => item.ProductVariantId).IsUnique();
        builder.HasIndex(item => item.SupplierId);

        builder.HasMany<StockReservation>("_reservations")
            .WithOne()
            .HasForeignKey(reservation => reservation.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_reservations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(item => item.Available);
        builder.Ignore(item => item.Reservations);
        builder.Ignore(item => item.DomainEvents);
    }
}

public sealed class StockReservationConfiguration
    : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations", "inventory");
        builder.HasKey(reservation => reservation.Id);
        // Client-generated Guids: without this, EF treats new reservations as Modified
        // and issues UPDATE 0 rows → DbUpdateConcurrencyException on place-order.
        builder.Property(reservation => reservation.Id).ValueGeneratedNever();

        builder.Property(reservation => reservation.Status)
            .HasConversion<string>()
            .HasMaxLength(24);

        builder.HasIndex(reservation => reservation.ReferenceId);
        builder.HasIndex(reservation => new
        {
            reservation.StockItemId,
            reservation.Status
        });
    }
}
