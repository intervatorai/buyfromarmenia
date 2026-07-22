using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Enums;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers", "suppliers");

        builder.HasKey(supplier => supplier.Id);

        builder.Property(supplier => supplier.LegalName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(supplier => supplier.TradingName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(supplier => supplier.TaxNumber)
            .HasMaxLength(64);

        builder.Property(supplier => supplier.RegistrationNumber)
            .HasMaxLength(64);

        builder.Property(supplier => supplier.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(supplier => supplier.DefaultPreparationTime)
            .IsRequired();

        builder.Property(supplier => supplier.CreatedAt)
            .IsRequired();

        builder.OwnsOne(supplier => supplier.Contact, contact =>
        {
            contact.Property(c => c.ContactPerson).HasColumnName("contact_person").HasMaxLength(200);
            contact.Property(c => c.Email).HasColumnName("contact_email").HasMaxLength(256);
            contact.Property(c => c.Phone).HasColumnName("contact_phone").HasMaxLength(32);
        });

        builder.OwnsOne(supplier => supplier.LegalAddress, address =>
        {
            address.Property(a => a.CountryCode).HasColumnName("legal_country_code").HasMaxLength(2);
            address.Property(a => a.City).HasColumnName("legal_city").HasMaxLength(100);
            address.Property(a => a.Line1).HasColumnName("legal_line1").HasMaxLength(300);
            address.Property(a => a.Line2).HasColumnName("legal_line2").HasMaxLength(300);
            address.Property(a => a.PostalCode).HasColumnName("legal_postal_code").HasMaxLength(20);
            address.Property(a => a.Region).HasColumnName("legal_region").HasMaxLength(100);
        });

        builder.OwnsOne(supplier => supplier.WarehouseAddress, address =>
        {
            address.Property(a => a.CountryCode).HasColumnName("warehouse_country_code").HasMaxLength(2);
            address.Property(a => a.City).HasColumnName("warehouse_city").HasMaxLength(100);
            address.Property(a => a.Line1).HasColumnName("warehouse_line1").HasMaxLength(300);
            address.Property(a => a.Line2).HasColumnName("warehouse_line2").HasMaxLength(300);
            address.Property(a => a.PostalCode).HasColumnName("warehouse_postal_code").HasMaxLength(20);
            address.Property(a => a.Region).HasColumnName("warehouse_region").HasMaxLength(100);
        });

        builder.HasMany<SupplierMember>("_members")
            .WithOne()
            .HasForeignKey(member => member.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<SupplierDocument>("_documents")
            .WithOne()
            .HasForeignKey(document => document.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<SupplierBankAccount>("_bankAccounts")
            .WithOne()
            .HasForeignKey(account => account.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_members").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_documents").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_bankAccounts").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(supplier => supplier.Members);
        builder.Ignore(supplier => supplier.Documents);
        builder.Ignore(supplier => supplier.BankAccounts);
        builder.Ignore(supplier => supplier.DomainEvents);
    }
}
