using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Column = WannabeTrello.Domain.Entities.Column;

namespace WannabeTrello.Infrastructure.Persistence.Configurations;

public class ColumnConfiguration : IEntityTypeConfiguration<Column>
{
    public void Configure(EntityTypeBuilder<Column> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Order)
            .IsRequired();

        
        builder.HasOne(c => c.Board)
            .WithMany(b => b.Columns)
            .HasForeignKey(c => c.BoardId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); 

        //Column ima više Zadataka
        builder.HasMany(c => c.Tasks)
            .WithOne(t => t.Column)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(c => c.WipLimit).HasDefaultValue(0);

        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        
        // Filter to exclude deleted columns from queries by default
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}