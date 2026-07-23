using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", "catalog");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(category => category.SkuPrefix)
            .IsRequired()
            .HasMaxLength(4)
            .HasDefaultValue(string.Empty);

        builder.HasIndex(category => category.SkuPrefix)
            .IsUnique()
            .HasFilter("\"SkuPrefix\" <> ''");

        builder.Property(category => category.SortOrder)
            .IsRequired();

        builder.Property(category => category.CreatedAt)
            .IsRequired();

        builder.HasMany<CategoryTranslation>("_translations")
            .WithOne()
            .HasForeignKey(translation => translation.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_translations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(category => category.Translations);
        builder.Ignore(category => category.DomainEvents);
    }
}
