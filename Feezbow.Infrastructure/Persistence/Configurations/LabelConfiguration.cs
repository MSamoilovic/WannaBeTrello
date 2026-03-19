using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Color)
            .IsRequired()
            .HasMaxLength(7);

        builder.HasOne(l => l.Board)
            .WithMany()
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.BoardId);
    }
}
