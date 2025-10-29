namespace WannabeTrello.Application.Common.Interfaces;

public interface ITaskNotificationService
{
    Task NotifyTaskCreated(long taskId, string taskTitle, long taskCreatorId, long? assigneeId);
    Task NotifyTaskUpdated(long taskId, string? taskTitle, long modifierUserId, Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues);
}