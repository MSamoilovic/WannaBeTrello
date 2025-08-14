using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectMemberAddedEvent(
    long projectId, 
    long newMemberId, 
    ProjectRole role, 
    long inviterUserId): DomainEvent
{
    public long ProjectId => projectId;
    public long NewMemberId => newMemberId;
    public ProjectRole Role => role;
    public long InviterUserId => inviterUserId;
}