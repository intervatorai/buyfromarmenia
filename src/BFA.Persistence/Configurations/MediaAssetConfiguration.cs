using BFA.Modules.Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets", "catalog");

        builder.HasKey(asset => asset.Id);
        builder.Property(asset => asset.Id).ValueGeneratedNever();

        builder.Property(asset => asset.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(asset => asset.StorageKey)
            .IsUnique();

        builder.Property(asset => asset.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(asset => asset.CreatedAt)
            .IsRequired();
    }
}
