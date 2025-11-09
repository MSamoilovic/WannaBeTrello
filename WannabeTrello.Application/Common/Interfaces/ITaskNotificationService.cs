namespace WannabeTrello.Application.Common.Interfaces;

public interface ITaskNotificationService
{
    Task NotifyTaskCreated(long taskId, string taskTitle, long taskCreatorId, long? assigneeId);
    Task NotifyTaskUpdated(long taskId, string? taskTitle, long modifierUserId, Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues);
    Task NotifyTaskMoved(long taskId, long newColumnId, long performedByUserId, CancellationToken cancellationToken);
    Task NotifyTaskAssigned(long boardId, long taskId, long? oldAssigneeId, long? newAssigneeId, long assignedByUserId, CancellationToken cancellationToken);
    Task NotifyTaskCommented(long boardId, long taskId, long commentId, long commentAuthorId, string content, CancellationToken cancellationToken); 
    Task NotifyCommentUpdated(long taskId, long commentId, long modifierUserId, Dictionary<string, object?> oldContent, Dictionary<string, object?> newContent, CancellationToken cancellationToken);
    Task NotifyCommentDeleted(long taskId, long commentId, long modifierUserId, CancellationToken cancellationToken);
    Task NotifyCommentRestored(long taskId, long commentId, long modifierUserId, CancellationToken cancellationToken);
}