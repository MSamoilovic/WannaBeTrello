using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectMemberUpdatedEvent(long id, long updatedMemberId, ProjectRole role, long inviterUserId) : DomainEvent
{
    public long Id => id;
    public long UpdatedMemberId => updatedMemberId;
    public ProjectRole Role => role;
    public long InviterUserId => inviterUserId;
}