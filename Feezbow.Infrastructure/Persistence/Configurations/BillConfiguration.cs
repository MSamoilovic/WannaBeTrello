using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.Property(b => b.Category)
            .HasMaxLength(50);

        builder.Property(b => b.Amount)
            .IsRequired()
            .HasPrecision(14, 2);

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(b => b.IsPaid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.NextOccurrence)
            .IsRequired(false);

        builder.HasIndex(b => b.ProjectId);
        builder.HasIndex(b => b.DueDate);
        builder.HasIndex(b => b.NextOccurrence);

        builder.HasOne(b => b.Project)
            .WithMany()
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.ParentBill)
            .WithMany()
            .HasForeignKey(b => b.ParentBillId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(b => b.Splits)
            .WithOne()
            .HasForeignKey(s => s.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Bill.Splits))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne(b => b.Recurrence, r =>
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
