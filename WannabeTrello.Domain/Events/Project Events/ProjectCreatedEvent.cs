namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectCreatedEvent(long projectId, string? projectName, long ownerId, string? projectDescription): DomainEvent
{
    public long ProjectId => projectId;
    public string? ProjectName => projectName;
    public long OwnerId => ownerId;
    public string? ProjectDescription => projectDescription;
}