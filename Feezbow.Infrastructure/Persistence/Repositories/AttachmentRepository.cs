using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class AttachmentRepository(ApplicationDbContext dbContext) : IAttachmentRepository
{
    private readonly DbSet<Attachment> _dbSet = dbContext.Set<Attachment>();

    public async Task<Attachment?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Attachment>> GetByOwnerAsync(
        AttachmentOwnerType ownerType,
        long ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(attachment, cancellationToken);
    }

    public void Remove(Attachment attachment)
    {
        _dbSet.Remove(attachment);
    }
}
