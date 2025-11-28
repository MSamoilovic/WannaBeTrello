using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration: IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        
        builder.HasKey(x => x.Id);

        //Activity as owned entity
        builder.OwnsOne(x => x.Activity, activity =>
        {
            activity.Property(a => a.Type)
                .HasColumnName("Activity_Type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            activity.Property(a => a.Description)
                .HasColumnName("Activity_Description")
                .HasMaxLength(500)
                .IsRequired();

            activity.Property(a => a.Timestamp)
                .HasColumnName("Activity_Timestamp")
                .IsRequired();

            activity.Property(a => a.UserId)
                .HasColumnName("Activity_UserId")
                .IsRequired();

            
            var jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            };
            
            activity.Property(a => a.OldValue)
                .HasColumnName("Activity_OldValue")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, jsonOptions) ?? new Dictionary<string, object?>()
                )
                .HasColumnType("jsonb");

            activity.Property(a => a.NewValue)
                .HasColumnName("Activity_NewValue")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, jsonOptions) ?? new Dictionary<string, object?>()
                )
                .HasColumnType("jsonb");
            
            // Indexes on owned entity properties - must be inside OwnsOne block
            activity.HasIndex(a => a.Timestamp)
                .HasDatabaseName("IX_ActivityLogs_Activity_Timestamp");
            
            activity.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_ActivityLogs_Activity_UserId");
        });

        
        builder.HasOne(x => x.BoardTask)
            .WithMany()
            .HasForeignKey(x => x.BoardTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Board)
            .WithMany()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes on foreign keys
        builder.HasIndex(x => x.BoardTaskId)
            .HasDatabaseName("IX_ActivityLogs_BoardTaskId");
        
        builder.HasIndex(x => x.ProjectId)
            .HasDatabaseName("IX_ActivityLogs_ProjectId");
        
        builder.HasIndex(x => x.BoardId)
            .HasDatabaseName("IX_ActivityLogs_BoardId");
    }
}