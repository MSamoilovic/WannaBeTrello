using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

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

    public async Task<Label?> GetByIdAsync(long labelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(l => l.Id == labelId, cancellationToken);
    }
}
