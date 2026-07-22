using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class SupplierDocumentConfiguration : IEntityTypeConfiguration<SupplierDocument>
{
    public void Configure(EntityTypeBuilder<SupplierDocument> builder)
    {
        builder.ToTable("supplier_documents", "suppliers");

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

        builder.Property(document => document.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);
    }
}
