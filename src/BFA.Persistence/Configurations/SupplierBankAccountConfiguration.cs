using BFA.Modules.Suppliers.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class SupplierBankAccountConfiguration : IEntityTypeConfiguration<SupplierBankAccount>
{
    public void Configure(EntityTypeBuilder<SupplierBankAccount> builder)
    {
        builder.ToTable("supplier_bank_accounts", "suppliers");

        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).ValueGeneratedNever();

        builder.Property(account => account.CreatedAt)
            .IsRequired();

        builder.OwnsOne(account => account.Details, details =>
        {
            details.Property(d => d.BankName).HasColumnName("bank_name").HasMaxLength(200);
            details.Property(d => d.AccountHolder).HasColumnName("account_holder").HasMaxLength(200);
            details.Property(d => d.Iban).HasColumnName("iban").HasMaxLength(64);
            details.Property(d => d.Swift).HasColumnName("swift").HasMaxLength(16);
            details.Property(d => d.Currency).HasColumnName("currency").HasMaxLength(3);
        });
    }
}
