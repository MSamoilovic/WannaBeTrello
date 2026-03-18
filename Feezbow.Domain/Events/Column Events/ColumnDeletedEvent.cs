namespace WannabeTrello.Domain.Events.Column_Events;

public class ColumnDeletedEvent(long columnId, long boardId, long modifierUserId): DomainEvent
{
    public long ColumnId => columnId;
    public long BoardId => boardId;
    public long ModifierUserId => modifierUserId;
}