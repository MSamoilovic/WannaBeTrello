using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class TaskNotificationService(
    IHubContext<TrellyHub, ITrellyHub> hubContext) : ITaskNotificationService
{
    public async Task NotifyTaskCreated(long taskId, string taskTitle, long taskCreatorId, long? assigneeId)
    {
        await hubContext.Clients.All.TaskCreated(taskId, taskTitle);
    }

    public async Task NotifyTaskUpdated(long taskId, string? taskTitle, long modifierUserId,
        Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues)
    {
        await hubContext.Clients.All.TaskUpdated(taskId.ToString(), newValues);
    }

    public async Task NotifyTaskMoved(long taskId, long newColumnId, long performedByUserId,
        CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.TaskMoved(taskId, newColumnId, performedByUserId);
    }

    public async Task NotifyTaskAssigned(long taskId, long? oldAssigneeId, long? newAssigneeId,
        long assignedByUserId, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.TaskAssigned(taskId.ToString(),
            newAssigneeId?.ToString() ?? string.Empty);

    }

    public async Task NotifyTaskCommented(long taskId, long commentId, long commentAuthorId,
        string content, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.CommentAdded(taskId.ToString(), commentId.ToString(),
            commentAuthorId.ToString(), content);

    }

    public async Task NotifyCommentUpdated(long taskId, long commentId, long modifierUserId, Dictionary<string, object?> oldContent,
        Dictionary<string, object?> newContent, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.CommentUpdated(taskId, commentId);
    }

    public async Task NotifyCommentDeleted(long taskId, long commentId, long modifierUserId, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.CommentDeleted(taskId, commentId);
    }

    public async Task NotifyCommentRestored(long taskId, long commentId, long modifierUserId, CancellationToken cancellationToken)
    {
        await hubContext.Clients.All.CommentRestored(taskId, commentId);
    }
}