namespace WannabeTrello.Domain.Events.Column_Events;

public class ColumnAddedEvent(long boardId, long columnId, string columnName, long creatorUserId)
    : DomainEvent
{
    public long BoardId => boardId;
    public long ColumnId => columnId;
    public string ColumnName => columnName;
    public long CreatorUserId => creatorUserId;
}