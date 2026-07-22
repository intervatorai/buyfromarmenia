using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("product_translations", "catalog");

        builder.HasKey(translation => translation.Id);
        builder.Property(translation => translation.Id).ValueGeneratedNever();

        builder.Property(translation => translation.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(translation => translation.ShortDescription)
            .HasMaxLength(500);

        builder.Property(translation => translation.Description)
            .HasMaxLength(4000);

        builder.Property(translation => translation.Ingredients)
            .HasMaxLength(2000);

        builder.Property(translation => translation.UsageInstructions)
            .HasMaxLength(2000);

        builder.Property(translation => translation.SeoTitle)
            .HasMaxLength(200);

        builder.Property(translation => translation.SeoDescription)
            .HasMaxLength(500);

        builder.Property(translation => translation.Language)
            .HasConversion(
                language => language.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(5)
            .HasColumnName("language_code");

        builder.HasIndex(translation => new { translation.ProductId, translation.Language })
            .IsUnique();
    }
}
