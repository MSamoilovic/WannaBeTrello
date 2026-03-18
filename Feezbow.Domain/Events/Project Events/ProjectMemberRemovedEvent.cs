using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectMemberRemovedEvent(long projectId, long removedUserId, ProjectRole removedUserRole, long removingUserId): DomainEvent
{
    public long ProjectId => projectId;
    public long RemovedUserId => removedUserId;
    public ProjectRole RemovedUserRole => removedUserRole;
    public long RemovingUserId => removingUserId;
}