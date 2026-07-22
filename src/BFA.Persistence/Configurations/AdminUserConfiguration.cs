using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users", "identity");

        builder.HasKey(user => user.Id);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(user => user.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(user => user.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Ignore(user => user.DomainEvents);
    }
}
