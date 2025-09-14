namespace WannabeTrello.Domain.Events;

public class BoardUpdatedEvent(
    long boardId,
    Dictionary<string, object?> oldValue,
    Dictionary<string, object?> newValue,
    long modifierUserId)
    : DomainEvent
{
    public long BoardId => boardId;
    public Dictionary<string, object?> OldValue => oldValue;
    public Dictionary<string, object?> NewValue => newValue;
    public long ModifierUserId => modifierUserId;
}