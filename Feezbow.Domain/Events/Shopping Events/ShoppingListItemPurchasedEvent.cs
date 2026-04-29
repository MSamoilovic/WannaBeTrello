namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListItemPurchasedEvent(long shoppingListId, long projectId, long itemId, long purchasedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ProjectId => projectId;
    public long ItemId => itemId;
    public long PurchasedBy => purchasedBy;
}
