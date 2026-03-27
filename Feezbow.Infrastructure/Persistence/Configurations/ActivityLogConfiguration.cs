using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

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

            var dictConverter = new ValueConverter<Dictionary<string, object?>, string>(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, jsonOptions) ?? new Dictionary<string, object?>()
            );

            var dictComparer = new ValueComparer<Dictionary<string, object?>>(
                (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions) ?? new Dictionary<string, object?>()
            );

            activity.Property(a => a.OldValue)
                .HasColumnName("Activity_OldValue")
                .HasConversion(dictConverter, dictComparer)
                .HasColumnType("jsonb");

            activity.Property(a => a.NewValue)
                .HasColumnName("Activity_NewValue")
                .HasConversion(dictConverter, dictComparer)
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