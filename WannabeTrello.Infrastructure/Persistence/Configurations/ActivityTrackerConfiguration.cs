using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Persistence.Configurations;

public class ActivityTrackerConfiguration: IEntityTypeConfiguration<ActivityTracker>
{
    public void Configure(EntityTypeBuilder<ActivityTracker> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(al => al.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(al => al.Timestamp)
            .IsRequired();

        builder.Property(al => al.UserId)
            .IsRequired(); 

        builder.HasOne(al => al.User)
            .WithMany() 
            .HasForeignKey(al => al.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Property(al => al.RelatedEntityId)
            .IsRequired(false); 

        builder.Property(al => al.RelatedEntityType)
            .HasMaxLength(100)
            .IsRequired(false);
        
        builder.Property(al => al.OldValue)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(al => al.NewValue)
            .HasColumnType("jsonb")
            .IsRequired(false);
    }
}