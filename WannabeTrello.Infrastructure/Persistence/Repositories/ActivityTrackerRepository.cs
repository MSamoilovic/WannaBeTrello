using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ActivityTrackerRepository(ApplicationDbContext dbContext)
    : Repository<ActivityTracker>(dbContext), IActivityTrackerRepository
{
    public async Task AddAsync(ActivityTracker activityTracker, CancellationToken cancellationToken)
    {
        await base.AddAsync(activityTracker);
    }

    public async Task<IEnumerable<ActivityTracker>> GetActivityForBoardAsync(long boardId,
        CancellationToken cancellationToken)
    {
        return await _dbSet.Where(at => at.RelatedEntityId == boardId && at.RelatedEntityType == nameof(Board))
            .OrderByDescending(at => at.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityTracker>> GetActivityForProjectAsync(long projectId,
        CancellationToken cancellationToken)
    {
        return await _dbSet.Where(at => at.RelatedEntityId == projectId && at.RelatedEntityType == nameof(Project))
            .OrderByDescending(at => at.Timestamp)
            .ToListAsync(cancellationToken);
    }
}