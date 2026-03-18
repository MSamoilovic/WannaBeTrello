namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectArchivedEvent(long projectId, string? projectName, long modifierUserId): DomainEvent
{
    public long ProjectId => projectId;
    public string? ProjectName => projectName;
    public long ModifierId => modifierUserId;
}