using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable("category_translations", "catalog");

        builder.HasKey(translation => translation.Id);

        builder.Property(translation => translation.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(translation => translation.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(translation => translation.Description)
            .HasMaxLength(1000);

        builder.Property(translation => translation.Language)
            .HasConversion(
                language => language.Value,
                value => LanguageCode.From(value))
            .HasMaxLength(5)
            .HasColumnName("language_code");

        builder.HasIndex(translation => new { translation.CategoryId, translation.Language })
            .IsUnique();

        builder.HasIndex(translation => new { translation.Slug, translation.Language })
            .IsUnique();
    }
}
