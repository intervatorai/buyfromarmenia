using BFA.Modules.Ordering.Domain.Aggregates;
using BFA.Modules.Ordering.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class CustomerOrderConfiguration : IEntityTypeConfiguration<CustomerOrder>
{
    public void Configure(EntityTypeBuilder<CustomerOrder> builder)
    {
        builder.ToTable("customer_orders", "ordering");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.OrderNumber).HasMaxLength(32).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.HasIndex(order => order.CartId);
        builder.HasIndex(order => order.CustomerUserId);
        builder.Property(order => order.CustomerEmail).HasMaxLength(320).IsRequired();
        builder.Property(order => order.CustomerFullName).HasMaxLength(200).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(order => order.PaymentStatus).HasConversion<string>().HasMaxLength(24);
        builder.Property(order => order.FulfillmentStatus).HasConversion<string>().HasMaxLength(24);
        builder.Property(order => order.TrackingStage).HasConversion<string>().HasMaxLength(32);
        builder.Property(order => order.Subtotal).HasPrecision(18, 2);
        builder.Property(order => order.EstimatedWeightKg).HasPrecision(18, 3);
        builder.Property(order => order.ShippingFeeQuoted).HasPrecision(18, 2);
        builder.Property(order => order.ShippingMarginPercent).HasPrecision(18, 2);
        builder.Property(order => order.ShippingFee).HasPrecision(18, 2);
        builder.Property(order => order.ShippingAdjustmentReason).HasMaxLength(500);
        builder.Ignore(order => order.Total);
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.Property(order => order.CreatedAtUtc).IsRequired();
        builder.Property(order => order.UpdatedAtUtc).IsRequired();

        builder.OwnsOne(order => order.ShippingAddress, address =>
        {
            address.Property(a => a.CountryCode).HasColumnName("shipping_country_code").HasMaxLength(2);
            address.Property(a => a.City).HasColumnName("shipping_city").HasMaxLength(120);
            address.Property(a => a.Line1).HasColumnName("shipping_line1").HasMaxLength(200);
            address.Property(a => a.Line2).HasColumnName("shipping_line2").HasMaxLength(200);
            address.Property(a => a.PostalCode).HasColumnName("shipping_postal_code").HasMaxLength(20);
            address.Property(a => a.Region).HasColumnName("shipping_region").HasMaxLength(120);
        });

        builder.HasMany<CustomerOrderItem>("_items")
            .WithOne()
            .HasForeignKey(item => item.CustomerOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(order => order.Items);
        builder.Ignore(order => order.DomainEvents);
    }
}

public sealed class CustomerOrderItemConfiguration : IEntityTypeConfiguration<CustomerOrderItem>
{
    public void Configure(EntityTypeBuilder<CustomerOrderItem> builder)
    {
        builder.ToTable("customer_order_items", "ordering");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(item => item.SupplierSku).HasMaxLength(64).IsRequired();
        builder.Property(item => item.ImageUrl).HasMaxLength(2048);
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.Currency).HasMaxLength(3).IsRequired();
        builder.Ignore(item => item.LineTotal);
    }
}
