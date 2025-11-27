using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Domain.Services;

public class ActivityLogService(IActivityLogRepository activityLogRepository, IUnitOfWork unitOfWork)
    : IActivityLogService
{
   
    public async Task<IEnumerable<Activity>> GetActivitiesForBoardAsync(long boardId, CancellationToken cancellationToken = default)
    {
        var activityLogs = await activityLogRepository.GetByBoardIdAsync(boardId, cancellationToken);
        return activityLogs.Select(al => al.Activity);
    }

    public async Task<IEnumerable<Activity>> GetActivitiesForProjectAsync(long projectId, CancellationToken cancellationToken = default)
    {
        var activityLogs = await activityLogRepository.GetByProjectIdAsync(projectId, cancellationToken);
        return activityLogs.Select(al => al.Activity);
    }

    public async Task<IEnumerable<Activity>> GetActivitiesForTaskAsync(long taskId, CancellationToken cancellationToken = default)
    {
        var activityLogs = await activityLogRepository.GetByTaskIdAsync(taskId, cancellationToken);
        return activityLogs.Select(al => al.Activity);
    }

    public async Task<ActivityLog> LogActivityAsync(Activity activity, long? boardTaskId = null, long? projectId = null, long? boardId = null, CancellationToken cancellationToken = default)
    {
        var activityLog = ActivityLog.CreateForTask(activity, boardTaskId ?? 0);

        if (projectId.HasValue)
            activityLog = ActivityLog.CreateForProject(activity, projectId.Value);

        if (boardId.HasValue)
            activityLog = ActivityLog.CreateForBoard(activity, boardId.Value);

        await activityLogRepository.AddAsync(activityLog, cancellationToken);
        return activityLog;
    }
}