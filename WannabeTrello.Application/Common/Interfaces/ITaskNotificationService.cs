namespace WannabeTrello.Application.Common.Interfaces;

public interface ITaskNotificationService
{
    Task NotifyTaskCreated(long taskId, string taskTitle, long taskCreatorId, long? assigneeId);
}