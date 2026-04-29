namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListItemUnpurchasedEvent(long shoppingListId, long projectId, long itemId, long updatedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ProjectId => projectId;
    public long ItemId => itemId;
    public long UpdatedBy => updatedBy;
}
