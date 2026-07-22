using BFA.Modules.Returns.Domain.Aggregates;
using BFA.Modules.Returns.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.ToTable("return_requests", "returns");
        builder.HasKey(request => request.Id);
        builder.HasIndex(request => request.CustomerOrderId);
        builder.HasIndex(request => request.Status);
        builder.Property(request => request.CustomerEmail).HasMaxLength(320).IsRequired();
        builder.Property(request => request.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(request => request.AdminNotes).HasMaxLength(1000);
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(24);
        builder.Ignore(request => request.DomainEvents);
    }
}
