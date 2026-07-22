using BFA.Modules.Shopping.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BFA.Persistence.Configurations;

public sealed class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.ToTable("shopping_carts", "shopping");
        builder.HasKey(cart => cart.Id);
        builder.Property(cart => cart.CreatedAtUtc).IsRequired();
        builder.Property(cart => cart.UpdatedAtUtc).IsRequired();

        builder.HasMany<ShoppingCartItem>("_items")
            .WithOne()
            .HasForeignKey(item => item.ShoppingCartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<WishlistItem>("_wishlistItems")
            .WithOne()
            .HasForeignKey(item => item.ShoppingCartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_wishlistItems").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(cart => cart.Items);
        builder.Ignore(cart => cart.WishlistItems);
        builder.Ignore(cart => cart.DomainEvents);
    }
}

public sealed class ShoppingCartItemConfiguration
    : IEntityTypeConfiguration<ShoppingCartItem>
{
    public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
    {
        builder.ToTable("shopping_cart_items", "shopping");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductName).HasMaxLength(300).IsRequired();
        builder.Property(item => item.ImageUrl).HasMaxLength(2048);
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.Currency).HasMaxLength(3).IsRequired();
        builder.Ignore(item => item.LineTotal);
        builder.HasIndex(item => new { item.ShoppingCartId, item.ProductVariantId })
            .IsUnique();
    }
}

public sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items", "shopping");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.ShoppingCartId, item.ProductId })
            .IsUnique();
    }
}
