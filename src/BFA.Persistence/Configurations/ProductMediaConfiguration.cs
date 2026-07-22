using BFA.Modules.Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
{
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        builder.ToTable("product_media", "catalog");

        builder.HasKey(media => media.Id);
        builder.Property(media => media.Id).ValueGeneratedNever();

        builder.Property(media => media.AltText)
            .HasMaxLength(300);

        builder.HasIndex(media => new { media.ProductId, media.MediaAssetId })
            .IsUnique();

        builder.HasOne(media => media.MediaAsset)
            .WithMany()
            .HasForeignKey(media => media.MediaAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
