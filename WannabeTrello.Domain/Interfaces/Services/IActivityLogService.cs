using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IActivityLogService
{
    Task<ActivityLog> LogActivityAsync(Activity activity, long? boardTaskId = null,
        long? projectId = null, long? boardId = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<Activity>> GetActivitiesForTaskAsync(long taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetActivitiesForProjectAsync(long projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetActivitiesForBoardAsync(long boardId, CancellationToken cancellationToken = default);
}