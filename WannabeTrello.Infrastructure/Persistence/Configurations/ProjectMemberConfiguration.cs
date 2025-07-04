using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Persistence.Configurations;

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
    }
}