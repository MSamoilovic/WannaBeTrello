namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListItemAddedEvent(long shoppingListId, long projectId, long itemId, string name, long addedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ProjectId => projectId;
    public long ItemId => itemId;
    public string Name => name;
    public long AddedBy => addedBy;
}
