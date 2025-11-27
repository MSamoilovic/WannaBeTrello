using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration: IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(x => x.Id);

        //Activity as owned entity
        builder.OwnsOne(x => x.Activity, activity =>
        {
            activity.Property(a => a.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            activity.Property(a => a.Description)
                .HasMaxLength(500)
                .IsRequired();

            activity.Property(a => a.Timestamp)
                .IsRequired();

            activity.Property(a => a.UserId)
                .IsRequired();

            
            activity.Property(a => a.OldValue)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (JsonSerializerOptions)null!)!
                )
                .HasColumnType("jsonb");

            activity.Property(a => a.NewValue)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (JsonSerializerOptions)null!)!
                )
                .HasColumnType("jsonb");
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

        builder.HasIndex(x => x.BoardTaskId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.BoardId);
        builder.HasIndex(x => x.Activity.Timestamp);
        builder.HasIndex(x => x.Activity.UserId);
    }
}