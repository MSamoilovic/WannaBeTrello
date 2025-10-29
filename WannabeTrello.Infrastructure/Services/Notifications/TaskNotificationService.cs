using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class TaskNotificationService(
    IHubContext<TrellyHub, ITrellyHub> hubContext,
    IActivityTrackerService activityTrackerService) : ITaskNotificationService
{
    public async Task NotifyTaskCreated(long taskId, string taskTitle, long taskCreatorId, long? assigneeId)
    {
        await hubContext.Clients.All.TaskCreated(taskId, taskTitle);
        
        var activity = ActivityTracker.Create(
            type: ActivityType.TaskCreated,
            description: $"Task '{taskTitle}' was created.",
            userId: taskCreatorId,
            relatedEntityId: taskId,
            relatedEntityType: nameof(BoardTask)
        );

        await activityTrackerService.AddActivityAsync(activity, CancellationToken.None);
    }

    public async Task NotifyTaskUpdated(long taskId, string? taskTitle, long modifierUserId, 
        Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues)
    {
        await hubContext.Clients.All.TaskUpdated("boardId", taskId.ToString(), newValues);
        
        var changedFields = string.Join(", ", newValues.Keys);
        var activity = ActivityTracker.Create(
            type: ActivityType.TaskUpdated,
            description: $"Task '{taskTitle}' was updated. Changed fields: {changedFields}",
            userId: modifierUserId,
            relatedEntityId: taskId,
            relatedEntityType: nameof(BoardTask),
            oldValue: oldValues,
            newValue: newValues
        );

        await activityTrackerService.AddActivityAsync(activity, CancellationToken.None);
    }
}