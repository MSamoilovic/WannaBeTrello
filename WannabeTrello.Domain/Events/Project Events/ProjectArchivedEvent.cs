namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectArchivedEvent(long projectId): DomainEvent
{
    public long ProjectId { get; } = projectId;
}