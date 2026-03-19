using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class TaskNotificationService(
    IHubContext<BoardHub, IBoardHubClient> boardHub,
    IHubContext<NotificationHub, INotificationHubClient> notificationHub,
    ResiliencePipeline pipeline,
    ILogger<TaskNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), ITaskNotificationService
{
    public async Task NotifyTaskCreated(long taskId, long boardId, string taskTitle, long taskCreatorId, long? assigneeId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .TaskCreated(new TaskCreatedNotification
            {
                TaskId = taskId,
                BoardId = boardId,
                TaskTitle = taskTitle,
                CreatedBy = taskCreatorId,
                AssigneeId = assigneeId
            })));
    }

    public async Task NotifyTaskUpdated(long taskId, long boardId, string? taskTitle, long modifierUserId,
        Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .TaskUpdated(new TaskUpdatedNotification
            {
                TaskId = taskId,
                BoardId = boardId,
                ModifiedBy = modifierUserId,
                Changes = newValues
            })));
    }

    public async Task NotifyTaskMoved(long taskId, long boardId, long newColumnId, long performedByUserId,
        CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .TaskMoved(new TaskMovedNotification
            {
                TaskId = taskId,
                BoardId = boardId,
                NewColumnId = newColumnId,
                MovedBy = performedByUserId
            })), cancellationToken);
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

        await SendAsync(_ => new ValueTask(boardHub.Clients.Group($"Board:{boardId}").TaskAssigned(notification)),
            cancellationToken);

        if (newAssigneeId.HasValue)
            await SendAsync(_ => new ValueTask(notificationHub.Clients.Group($"User:{newAssigneeId}").TaskAssigned(notification)),
                cancellationToken);
    }

    public async Task NotifyTaskCommented(long taskId, long boardId, long commentId, long commentAuthorId,
        string content, CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentAdded(new CommentAddedNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                AuthorId = commentAuthorId,
                Content = content
            })), cancellationToken);
    }

    public async Task NotifyCommentUpdated(long taskId, long boardId, long commentId, long modifierUserId,
        Dictionary<string, object?> oldContent, Dictionary<string, object?> newContent,
        CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentUpdated(new CommentUpdatedNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                ModifiedBy = modifierUserId
            })), cancellationToken);
    }

    public async Task NotifyCommentDeleted(long taskId, long boardId, long commentId, long modifierUserId,
        CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentDeleted(new CommentDeletedNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                DeletedBy = modifierUserId
            })), cancellationToken);
    }

    public async Task NotifyCommentRestored(long taskId, long boardId, long commentId, long modifierUserId,
        CancellationToken cancellationToken)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .CommentRestored(new CommentRestoredNotification
            {
                TaskId = taskId,
                CommentId = commentId,
                RestoredBy = modifierUserId
            })), cancellationToken);
    }
}
