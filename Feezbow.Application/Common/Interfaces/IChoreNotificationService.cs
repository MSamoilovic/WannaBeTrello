namespace Feezbow.Application.Common.Interfaces;

public interface IChoreNotificationService
{
    Task NotifyChoreCreated(long choreId, long projectId, string title, long? assignedToUserId, long createdBy, CancellationToken cancellationToken = default);
    Task NotifyChoreUpdated(long choreId, long projectId, long modifiedBy, CancellationToken cancellationToken = default);
    Task NotifyChoreAssigned(long choreId, long projectId, long? assignedToUserId, long? previousUserId, long assignedBy, CancellationToken cancellationToken = default);
    Task NotifyChoreCompleted(long choreId, long projectId, long completedBy, CancellationToken cancellationToken = default);
    Task NotifyChoreDeleted(long choreId, long projectId, long deletedBy, CancellationToken cancellationToken = default);
}
