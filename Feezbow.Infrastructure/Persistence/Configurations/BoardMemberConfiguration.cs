using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Infrastructure.Persistence.Configurations;

public class BoardMemberConfiguration : IEntityTypeConfiguration<BoardMember>
{
    public void Configure(EntityTypeBuilder<BoardMember> builder)
    {
        
        builder.HasKey(bm => new { bm.BoardId, bm.UserId }); 
        
        builder.HasOne(bm => bm.Board)
            .WithMany(b => b.BoardMembers)
            .HasForeignKey(bm => bm.BoardId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(bm => bm.User)
            .WithMany(u => u.BoardMemberships)
            .HasForeignKey(bm => bm.UserId)
            .OnDelete(DeleteBehavior.NoAction); 
        
        builder.Property(bm => bm.Role)
            .IsRequired()
            .HasConversion<string>(); 
    }
}
