using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IActivityTrackerRepository
{
    Task AddAsync(ActivityTracker activityTracker, CancellationToken cancellationToken);
    Task<IEnumerable<ActivityTracker>> GetActivityForBoardAsync(long boardId, CancellationToken cancellationToken);
    Task<IEnumerable<ActivityTracker>> GetActivityProjectAsync(long projectId, CancellationToken cancellationToken);
}