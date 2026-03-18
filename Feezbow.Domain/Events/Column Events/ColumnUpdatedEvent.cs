namespace WannabeTrello.Domain.Events.Column_Events;

public class ColumnUpdatedEvent(long columnId, string oldName, string newName, long boardId, long modifierUserId) : DomainEvent
{
    public long ColumnId => columnId;
    public string OldName => oldName;
    public string NewName => newName;
    public long BoardId => boardId;
    public long ModifierUserId => modifierUserId;
}
