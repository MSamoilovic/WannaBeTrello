namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListItemRemovedEvent(long shoppingListId, long itemId, long removedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ItemId => itemId;
    public long RemovedBy => removedBy;
}
