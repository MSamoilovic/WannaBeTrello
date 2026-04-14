using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class HouseholdProfileConfiguration : IEntityTypeConfiguration<HouseholdProfile>
{
    public void Configure(EntityTypeBuilder<HouseholdProfile> builder)
    {
        builder.HasKey(h => h.Id);

        builder.HasIndex(h => h.ProjectId)
            .IsUnique(); // 1:1 — jedan Project može imati jedan HouseholdProfile

        builder.Property(h => h.Address)
            .HasMaxLength(200);

        builder.Property(h => h.City)
            .HasMaxLength(100);

        builder.Property(h => h.Timezone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Europe/Belgrade");

        builder.Property(h => h.ShoppingDay)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(h => h.Project)
            .WithOne()
            .HasForeignKey<HouseholdProfile>(h => h.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
