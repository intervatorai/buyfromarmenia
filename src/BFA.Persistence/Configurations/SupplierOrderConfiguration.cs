using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Fulfillment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class SupplierOrderConfiguration : IEntityTypeConfiguration<SupplierOrder>
{
    public void Configure(EntityTypeBuilder<SupplierOrder> builder)
    {
        builder.ToTable("supplier_orders", "fulfillment");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(order => order.Subtotal).HasPrecision(18, 2);
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.Property(order => order.CreatedAtUtc).IsRequired();
        builder.Property(order => order.UpdatedAtUtc).IsRequired();
        builder.HasIndex(order => order.SupplierId);
        builder.HasIndex(order => order.CustomerOrderId);

        builder.HasMany<SupplierOrderItem>("_items")
            .WithOne()
            .HasForeignKey(item => item.SupplierOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(order => order.Items);
        builder.Ignore(order => order.DomainEvents);
    }
}

public sealed class SupplierOrderItemConfiguration : IEntityTypeConfiguration<SupplierOrderItem>
{
    public void Configure(EntityTypeBuilder<SupplierOrderItem> builder)
    {
        builder.ToTable("supplier_order_items", "fulfillment");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(item => item.SupplierSku).HasMaxLength(64).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.Currency).HasMaxLength(3).IsRequired();
        builder.Ignore(item => item.LineTotal);
    }
}
