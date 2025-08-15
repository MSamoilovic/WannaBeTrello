namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectArchivedEvent(long projectId, long modifierUserId): DomainEvent
{
    public long ProjectId { get; } = projectId;
    public long ModifierId { get; } = modifierUserId;
}