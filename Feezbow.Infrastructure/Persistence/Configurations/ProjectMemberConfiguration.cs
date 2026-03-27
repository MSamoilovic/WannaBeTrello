using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
       
        builder.HasKey(pm => new { pm.ProjectId, pm.UserId });
        
        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.ProjectMembers)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade); 
        
        builder.HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Property(pm => pm.Role)
            .IsRequired()
            .HasConversion<string>(); // Čuva enum kao string u bazi

        // Match Project's query filter so required navigation is never unexpectedly null
        builder.HasQueryFilter(pm => !pm.Project.IsArchived);
    }
}