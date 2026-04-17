using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
{
    public void Configure(EntityTypeBuilder<ShoppingListItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .HasPrecision(12, 3);

        builder.Property(i => i.Unit)
            .HasMaxLength(50);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.Property(i => i.IsPurchased)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(i => i.ShoppingListId);
    }
}
