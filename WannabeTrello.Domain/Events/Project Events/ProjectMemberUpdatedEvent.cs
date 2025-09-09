using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectMemberUpdatedEvent(
    long id,
    string? projectName,
    long updatedMemberId,
    ProjectRole oldRole,
    ProjectRole newRole,
    long inviterUserId) : DomainEvent
{
    public long Id => id;
    public string? ProjectName => projectName;
    public long UpdatedMemberId => updatedMemberId;
    public ProjectRole OldRole => oldRole;
    public ProjectRole NewRole => newRole;
    public long InviterUserId => inviterUserId;
}