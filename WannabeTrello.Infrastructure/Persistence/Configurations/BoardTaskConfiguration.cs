using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Persistence.Configurations;

public class BoardTaskConfiguration: IEntityTypeConfiguration<BoardTask>
{
    public void Configure(EntityTypeBuilder<BoardTask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500); 

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.DueDate)
            .IsRequired(false);
        
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
    }
}