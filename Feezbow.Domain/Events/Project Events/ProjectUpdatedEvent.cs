namespace WannabeTrello.Domain.Events.Project_Events;

public class ProjectUpdatedEvent(
    long id,
    string? name,
    long modifierId,
    Dictionary<string, object?>? oldValue = null,
    Dictionary<string, object?>? newValue = null) : DomainEvent
{
    public long Id => id;
    public string? Name => name;
    public long ModifierId => modifierId;
    public Dictionary<string, object?>? OldValue => oldValue;
    public Dictionary<string, object?>? NewValue => newValue;
}