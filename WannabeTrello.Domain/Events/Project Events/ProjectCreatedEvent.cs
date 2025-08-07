namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectCreatedEvent(long projectId, string? projectName, long ownerId): DomainEvent
{
    public long ProjectId { get; } = projectId;
    public string? ProjectName { get; } = projectName;
    public long OwnerId { get; } = ownerId;
}