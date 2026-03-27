using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class LabelRepository(ApplicationDbContext dbContext)
    : Repository<Label>(dbContext), ILabelRepository
{
    public async Task<IReadOnlyList<Label>> GetByBoardIdAsync(long boardId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(l => l.BoardId == boardId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Label?> GetByIdAsync(long labelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.Id == labelId, cancellationToken);
    }
}
