namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectMemberRemovedEvent(long projectId, long removedUserId, long removingUserId): DomainEvent
{
    public long ProjectId => projectId;
    public long RemovedUserId => removedUserId;
    public long RemovingUserId => removingUserId;
}