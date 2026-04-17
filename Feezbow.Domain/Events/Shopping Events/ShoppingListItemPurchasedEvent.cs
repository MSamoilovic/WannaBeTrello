namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListItemPurchasedEvent(long shoppingListId, long itemId, long purchasedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ItemId => itemId;
    public long PurchasedBy => purchasedBy;
}
