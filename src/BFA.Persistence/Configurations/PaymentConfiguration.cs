using BFA.Modules.Payments.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", "payments");
        builder.HasKey(payment => payment.Id);
        builder.HasIndex(payment => payment.CustomerOrderId).IsUnique();
        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
        builder.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(payment => payment.Provider).HasConversion<string>().HasMaxLength(24);
        builder.Property(payment => payment.ExternalReference).HasMaxLength(128);
        builder.Property(payment => payment.CreatedAtUtc).IsRequired();
        builder.Ignore(payment => payment.DomainEvents);
    }
}
