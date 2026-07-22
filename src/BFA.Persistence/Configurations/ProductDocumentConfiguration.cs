using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class ProductDocumentConfiguration : IEntityTypeConfiguration<ProductDocument>
{
    public void Configure(EntityTypeBuilder<ProductDocument> builder)
    {
        builder.ToTable("product_documents", "catalog");

        builder.HasKey(document => document.Id);
        builder.Property(document => document.Id).ValueGeneratedNever();

        builder.Property(document => document.FileName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(document => document.FileUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(document => document.DocumentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);
    }
}
