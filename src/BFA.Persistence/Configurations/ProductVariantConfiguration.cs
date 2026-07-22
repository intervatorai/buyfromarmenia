using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants", "catalog");

        builder.HasKey(variant => variant.Id);

        // Client-generated Guids: if ValueGeneratedOnAdd remains, DetectChanges treats
        // newly added variants on an existing product as Modified and issues UPDATE → 0 rows.
        builder.Property(variant => variant.Id)
            .ValueGeneratedNever();

        builder.Property(variant => variant.SupplierSku)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(variant => variant.Barcode)
            .HasMaxLength(64);

        builder.Property(variant => variant.Size)
            .HasMaxLength(32);

        builder.Property(variant => variant.Color)
            .HasMaxLength(32);

        builder.Property(variant => variant.Weight)
            .HasPrecision(10, 3);

        builder.Property(variant => variant.CustomsCode)
            .HasMaxLength(32);

        builder.Property(variant => variant.CountryOfOrigin)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(variant => variant.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.OwnsOne(variant => variant.Dimensions, dimensions =>
        {
            dimensions.Property(d => d.Length).HasColumnName("length").HasPrecision(10, 2);
            dimensions.Property(d => d.Width).HasColumnName("width").HasPrecision(10, 2);
            dimensions.Property(d => d.Height).HasColumnName("height").HasPrecision(10, 2);
            dimensions.Property(d => d.Unit).HasColumnName("dimension_unit").HasMaxLength(8);
        });

        builder.HasIndex(variant => new { variant.ProductId, variant.SupplierSku })
            .IsUnique();
    }
}
