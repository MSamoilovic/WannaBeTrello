namespace WannabeTrello.Domain.Events.Column_Events;

public class ColumnWipLimitChangedEvent(long columnId, long boardId, int? oldWipLimit, int? newWipLimit, long modifierUserId) : DomainEvent
{
    public long ColumnId => columnId;
    public long BoardId => boardId;
    public int? OldWipLimit => oldWipLimit;
    public int? NewWipLimit => newWipLimit;
    public long ModifierUserId => modifierUserId;
}
