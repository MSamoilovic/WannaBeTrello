using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class HouseholdChoreRepository(ApplicationDbContext dbContext) : IHouseholdChoreRepository
{
    private readonly DbSet<HouseholdChore> _dbSet = dbContext.Set<HouseholdChore>();

    public async Task<HouseholdChore?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<HouseholdChore>> GetByProjectAsync(long projectId, bool includeCompleted, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(c => c.AssignedTo)
            .Where(c => c.ProjectId == projectId);

        if (!includeCompleted)
            query = query.Where(c => !c.IsCompleted);

        return await query
            .OrderBy(c => c.DueDate)
            .ThenByDescending(c => c.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(HouseholdChore chore, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(chore, cancellationToken);
    }

    public void Remove(HouseholdChore chore)
    {
        _dbSet.Remove(chore);
    }
}
