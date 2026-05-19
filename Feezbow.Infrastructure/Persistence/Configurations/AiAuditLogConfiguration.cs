using Feezbow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class AiAuditLogConfiguration : IEntityTypeConfiguration<AiAuditLog>
{
    public void Configure(EntityTypeBuilder<AiAuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AgentName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.InputHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}