namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectUpdatedEvent(long id, string? name, long modifierId): DomainEvent
{
    public long Id => id;
    public string? Name => name;
    public long ModifierId => modifierId;
}