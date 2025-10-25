namespace WannabeTrello.Domain.Events.Column_Events;

public class ColumnOrderChangedEvent(long columnId, long boardId, int oldOrder, int newOrder, long modifierUserId) : DomainEvent
{
    public long ColumnId => columnId;
    public long BoardId => boardId;
    public int OldOrder => oldOrder;
    public int NewOrder => newOrder;
    public long ModifierUserId => modifierUserId;
}
