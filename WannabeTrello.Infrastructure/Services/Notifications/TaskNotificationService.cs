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
    public async Task NotifyTaskCreated(long taskId, string taskTitle, long taskCreatorId, long assigneeId)
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
}