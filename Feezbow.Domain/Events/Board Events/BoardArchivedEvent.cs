namespace WannabeTrello.Domain.Events.Board_Events;

public class BoardArchivedEvent(long boardId, long modifierUserId): DomainEvent
{
    public long BoardId => boardId;
    public long ModifierUserId => modifierUserId;
}