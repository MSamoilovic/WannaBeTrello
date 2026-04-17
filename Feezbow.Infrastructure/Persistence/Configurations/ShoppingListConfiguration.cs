using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingList>
{
    public void Configure(EntityTypeBuilder<ShoppingList> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(l => l.ProjectId);

        builder.HasOne(l => l.Project)
            .WithMany()
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Items)
            .WithOne()
            .HasForeignKey(i => i.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(ShoppingList.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
