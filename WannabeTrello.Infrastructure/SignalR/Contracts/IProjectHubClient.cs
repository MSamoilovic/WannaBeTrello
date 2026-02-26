namespace WannabeTrello.Infrastructure.SignalR.Contracts;

/// <summary>
/// Strongly-typed client contract for project-related real-time notifications.
/// Clients subscribed to project groups receive these events.
/// </summary>
public interface IProjectHubClient
{
    Task ProjectCreated(ProjectCreatedNotification notification);
    Task ProjectUpdated(ProjectUpdatedNotification notification);
    Task ProjectArchived(ProjectArchivedNotification notification);
    Task MemberAdded(ProjectMemberAddedNotification notification);
    Task MemberRemoved(ProjectMemberRemovedNotification notification);
    Task MemberUpdated(ProjectMemberUpdatedNotification notification);
}
