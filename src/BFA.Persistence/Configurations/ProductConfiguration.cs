using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.SupplierId)
            .IsRequired();

        builder.Property(product => product.ProductType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(product => product.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(product => product.Tag)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(ProductTag.None);

        builder.Property(product => product.DefaultLanguage)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(product => product.Slug)
            .IsRequired()
            .HasMaxLength(160);

        builder.HasIndex(product => product.Slug)
            .IsUnique();

        builder.Property(product => product.SearchKeywords)
            .HasMaxLength(500);

        builder.Property(product => product.SearchText)
            .IsRequired()
            .HasMaxLength(4000)
            .HasDefaultValue(string.Empty);

        builder.Property(product => product.CreatedAt)
            .IsRequired();

        builder.OwnsOne(product => product.BasePrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("base_price_amount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("base_price_currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(product => product.ShippingProfile, profile =>
        {
            profile.Property(p => p.NetWeight).HasColumnName("shipping_net_weight").HasPrecision(10, 3);
            profile.Property(p => p.GrossWeight).HasColumnName("shipping_gross_weight").HasPrecision(10, 3);
            profile.Property(p => p.PackageLength).HasColumnName("shipping_length").HasPrecision(10, 2);
            profile.Property(p => p.PackageWidth).HasColumnName("shipping_width").HasPrecision(10, 2);
            profile.Property(p => p.PackageHeight).HasColumnName("shipping_height").HasPrecision(10, 2);
            profile.Property(p => p.PackageDimensionUnit).HasColumnName("shipping_dimension_unit").HasMaxLength(8);
            profile.Property(p => p.IsFragile).HasColumnName("shipping_is_fragile");
            profile.Property(p => p.IsPerishable).HasColumnName("shipping_is_perishable");
            profile.Property(p => p.RequiresCooling).HasColumnName("shipping_requires_cooling");
            profile.Property(p => p.ContainsLiquid).HasColumnName("shipping_contains_liquid");
            profile.Property(p => p.ContainsAlcohol).HasColumnName("shipping_contains_alcohol");
            profile.Property(p => p.ContainsBattery).HasColumnName("shipping_contains_battery");
            profile.Property(p => p.DangerousGoodsCode).HasColumnName("shipping_dangerous_goods_code").HasMaxLength(32);
        });

        builder.HasMany<ProductTranslation>("_translations")
            .WithOne()
            .HasForeignKey(translation => translation.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ProductVariant>("_variants")
            .WithOne()
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ProductMedia>("_media")
            .WithOne()
            .HasForeignKey(media => media.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<ProductDocument>("_documents")
            .WithOne()
            .HasForeignKey(document => document.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_translations").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_variants").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_media").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_documents").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(product => product.Translations);
        builder.Ignore(product => product.Variants);
        builder.Ignore(product => product.Media);
        builder.Ignore(product => product.Documents);
        builder.Ignore(product => product.DomainEvents);
    }
}
