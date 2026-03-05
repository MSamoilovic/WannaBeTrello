using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR.Contracts;
using WannabeTrello.Infrastructure.SignalR.Hubs;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class TaskNotificationService(
    IHubContext<BoardHub, IBoardHubClient> boardHub,
    IHubContext<NotificationHub, INotificationHubClient> notificationHub) : ITaskNotificationService
{
    public async Task NotifyTaskCreated(long taskId, long boardId, string taskTitle, long taskCreatorId, long? assigneeId)
    {
        await boardHub.Clients
            .Group($"Board:{boardId}")
            .TaskCreated(new TaskCreatedNotification
            {
                TaskId = taskId,
                BoardId = boardId,
                TaskTitle = taskTitle,
                CreatedBy = taskCreatorId,
                AssigneeId = assigneeId
            });
    }

    public async Task NotifyTaskUpdated(long taskId, long boardId, string? taskTitle, long modifierUserId,
        Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues)
    {
        await boardHub.Clients
            .Group($"Board:{boardId}")
            .TaskUpdated(new TaskUpdatedNotification
            {
                TaskId = taskId,
                BoardId = boardId,
                ModifiedBy = modifierUserId,
                Changes = newValues
            });
    }

    public async Task NotifyTaskMoved(long taskId, long boardId, long newColumnId, long performedByUserId,
        CancellationToken cancellationToken)
    {
        await boardHub.Clients
            .Group($"Board:{boardId}")
            .TaskMoved(new TaskMovedNotification
            {
                TaskId = taskId,
                BoardId = boardId,
                NewColumnId = newColumnId,
                MovedBy = performedByUserId
            });
    }

    public async Task NotifyTaskAssigned(long taskId, long boardId, long? oldAssigneeId, long? newAssigneeId,
        long assignedByUserId, CancellationToken cancellationToken)
    {
        var notification = new TaskAssignedNotification
        {
            TaskId = taskId,
            BoardId = boardId,
            OldAssigneeId = oldAssigneeId,
            NewAssigneeId = newAssigneeId,
            AssignedBy = assignedByUserId
        };

        await boardHub.Clients.Group($"Board:{boardId}").TaskAssigned(notification);

        if (newAssigneeId.HasValue)
            await notificationHub.Clients.Group($"User:{newAssigneeId}").TaskAssigned(notification);
    }

    public async Task NotifyTaskCommented(long taskId, long boardId, long commentId, long commentAuthorId,
        string content, CancellationToken cancellationToken)
    {
        await boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentAdded(new CommentAddedNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                AuthorId = commentAuthorId,
                Content = content
            });
    }

    public async Task NotifyCommentUpdated(long taskId, long boardId, long commentId, long modifierUserId,
        Dictionary<string, object?> oldContent, Dictionary<string, object?> newContent,
        CancellationToken cancellationToken)
    {
        await boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentUpdated(new CommentUpdatedNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                ModifiedBy = modifierUserId
            });
    }

    public async Task NotifyCommentDeleted(long taskId, long boardId, long commentId, long modifierUserId,
        CancellationToken cancellationToken)
    {
        await boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentDeleted(new CommentDeletedNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                DeletedBy = modifierUserId
            });
    }

    public async Task NotifyCommentRestored(long taskId, long boardId, long commentId, long modifierUserId,
        CancellationToken cancellationToken)
    {
        await boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentRestored(new CommentRestoredNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                RestoredBy = modifierUserId
            });
    }
}