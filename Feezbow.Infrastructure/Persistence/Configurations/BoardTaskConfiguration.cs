using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class BoardTaskConfiguration: IEntityTypeConfiguration<BoardTask>
{
    public void Configure(EntityTypeBuilder<BoardTask> builder)
    {
        builder.HasKey(t => t.Id);
        
        // Ignore Activities - it's a transient collection, not persisted
        builder.Ignore(t => t.Activities);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500); 

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.TaskType)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(TaskType.General);

        builder.Property(t => t.DueDate)
            .IsRequired(false);

        builder.Property(t => t.NextOccurrence)
            .IsRequired(false);

        // Self-referential FK: occurrence → parent recurring task
        builder.HasOne(t => t.ParentTask)
            .WithMany()
            .HasForeignKey(t => t.ParentTaskId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // RecurrenceRule stored as owned entity (columns in same table)
        builder.OwnsOne(t => t.Recurrence, r =>
        {
            r.Property(x => x.Frequency)
                .HasColumnName("Recurrence_Frequency")
                .HasConversion<string>();

            r.Property(x => x.Interval)
                .HasColumnName("Recurrence_Interval")
                .HasDefaultValue(1);

            r.Property(x => x.DaysOfWeek)
                .HasColumnName("Recurrence_DaysOfWeek")
                .HasMaxLength(100)
                .IsRequired(false);

            r.Property(x => x.EndDate)
                .HasColumnName("Recurrence_EndDate")
                .IsRequired(false);
        });

        builder.HasOne(t => t.Column)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.ColumnId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); 

        
        builder.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks) 
            .HasForeignKey(t => t.AssigneeId)
            .IsRequired(false) 
            .OnDelete(DeleteBehavior.Restrict);

        
        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Task)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Exclude archived tasks and tasks on deleted columns
        builder.HasQueryFilter(t => !t.IsArchived && !t.Column.IsDeleted);
    }
}