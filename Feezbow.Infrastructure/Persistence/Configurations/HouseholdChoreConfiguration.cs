using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class HouseholdChoreConfiguration : IEntityTypeConfiguration<HouseholdChore>
{
    public void Configure(EntityTypeBuilder<HouseholdChore> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TaskPriority.Medium);

        builder.HasIndex(c => c.ProjectId);
        builder.HasIndex(c => c.AssignedToUserId);

        builder.HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AssignedTo)
            .WithMany()
            .HasForeignKey(c => c.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(c => c.Recurrence, r =>
        {
            r.Property(x => x.Frequency)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnName("RecurrenceFrequency");

            r.Property(x => x.Interval)
                .HasColumnName("RecurrenceInterval");

            r.Property(x => x.DaysOfWeek)
                .HasMaxLength(100)
                .HasColumnName("RecurrenceDaysOfWeek");

            r.Property(x => x.EndDate)
                .HasColumnName("RecurrenceEndDate");
        });
    }
}
