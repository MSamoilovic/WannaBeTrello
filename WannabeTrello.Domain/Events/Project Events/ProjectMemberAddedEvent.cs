using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectMemberAddedEvent(
    long projectId,
    string? projectName,
    long newMemberId, 
    ProjectRole role, 
    long inviterUserId): DomainEvent
{
    public long ProjectId => projectId;
    public string? ProjectName => projectName;
    public long NewMemberId => newMemberId;
    public ProjectRole Role => role;
    public long InviterUserId => inviterUserId;
}