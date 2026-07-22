using BFA.Modules.Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");
        builder.HasKey(user => user.Id);
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(user => user.Type).HasConversion<string>().HasMaxLength(24);
        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(24);
        builder.Ignore(user => user.DomainEvents);
    }
}

public sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("customer_profiles", "identity");
        builder.HasKey(profile => profile.Id);
        builder.HasIndex(profile => profile.UserId).IsUnique();
        builder.Property(profile => profile.FullName).HasMaxLength(200).IsRequired();
        builder.Property(profile => profile.Phone).HasMaxLength(32);
        builder.Ignore(profile => profile.DomainEvents);
    }
}

public sealed class CustomerDeliveryAddressConfiguration
    : IEntityTypeConfiguration<CustomerDeliveryAddress>
{
    public void Configure(EntityTypeBuilder<CustomerDeliveryAddress> builder)
    {
        builder.ToTable("customer_delivery_addresses", "identity");
        builder.HasKey(address => address.Id);
        builder.Property(address => address.Id).ValueGeneratedNever();
        builder.HasIndex(address => address.UserId);
        builder.HasIndex(address => new { address.UserId, address.IsDefault });
        builder.Property(address => address.Label).HasMaxLength(80).IsRequired();
        builder.Property(address => address.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(address => address.City).HasMaxLength(120).IsRequired();
        builder.Property(address => address.Line1).HasMaxLength(300).IsRequired();
        builder.Property(address => address.Line2).HasMaxLength(300);
        builder.Property(address => address.PostalCode).HasMaxLength(32);
        builder.Property(address => address.Region).HasMaxLength(120);
        builder.Ignore(address => address.DomainEvents);
    }
}
