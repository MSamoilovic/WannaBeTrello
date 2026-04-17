namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListItemUnpurchasedEvent(long shoppingListId, long itemId, long updatedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ItemId => itemId;
    public long UpdatedBy => updatedBy;
}
