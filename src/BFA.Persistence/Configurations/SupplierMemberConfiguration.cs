using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class SupplierMemberConfiguration : IEntityTypeConfiguration<SupplierMember>
{
    public void Configure(EntityTypeBuilder<SupplierMember> builder)
    {
        builder.ToTable("supplier_members", "suppliers");

        builder.HasKey(member => member.Id);
        builder.Property(member => member.Id).ValueGeneratedNever();

        builder.Property(member => member.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(member => member.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(member => member.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(member => new { member.SupplierId, member.Email })
            .IsUnique();
    }
}
