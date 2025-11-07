using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
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

    public async Task NotifyTaskMoved(long taskId, long newColumnId, long performedByUserId,
        CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.TaskMoved(taskId, newColumnId, performedByUserId);

        var activity = ActivityTracker.Create(
            type: ActivityType.TaskMoved,
            description: $"Task '{taskId}' was moved to column {newColumnId}.",
            userId: performedByUserId,
            relatedEntityId: taskId,
            relatedEntityType: nameof(BoardTask)
        );

        await activityTrackerService.AddActivityAsync(activity, CancellationToken.None);
    }

    public async Task NotifyTaskAssigned(long boardId, long taskId, long? oldAssigneeId, long? newAssigneeId,
        long assignedByUserId, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.TaskAssigned(boardId.ToString(), taskId.ToString(),
            newAssigneeId?.ToString() ?? string.Empty);

        var description = newAssigneeId.HasValue
            ? $"Task '{taskId}' was assigned to user {newAssigneeId}."
            : $"Task '{taskId}' was unassigned.";

        var activity = ActivityTracker.Create(
            type: ActivityType.TaskAssigned,
            description: description,
            userId: assignedByUserId,
            relatedEntityId: taskId,
            relatedEntityType: nameof(BoardTask),
            oldValue: new Dictionary<string, object?> { { nameof(BoardTask.AssigneeId), oldAssigneeId } },
            newValue: new Dictionary<string, object?> { { nameof(BoardTask.AssigneeId), newAssigneeId } }
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }

    public async Task NotifyTaskCommented(long boardId, long taskId, long commentId, long commentAuthorId,
        string content, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.CommentAdded(taskId.ToString(), commentId.ToString(),
            commentAuthorId.ToString(), content);

        var activity = ActivityTracker.Create(
            type: ActivityType.CommentAdded,
            description: $"User {commentAuthorId} added a comment to task '{taskId}' on board {boardId}.",
            userId: commentAuthorId,
            relatedEntityId: taskId,
            relatedEntityType: nameof(BoardTask),
            newValue: new Dictionary<string, object?>
            {
                { "CommentId", commentId },
                { "Content", content }
            }
        );

        await activityTrackerService.AddActivityAsync(activity, cancellationToken);
    }
}