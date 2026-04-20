using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class BillSplitConfiguration : IEntityTypeConfiguration<BillSplit>
{
    public void Configure(EntityTypeBuilder<BillSplit> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Amount)
            .IsRequired()
            .HasPrecision(14, 2);

        builder.Property(s => s.IsPaid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(s => s.BillId);
        builder.HasIndex(s => s.UserId);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
