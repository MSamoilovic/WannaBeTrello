namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListItemUpdatedEvent(
    long shoppingListId,
    long itemId,
    long updatedBy,
    IDictionary<string, object?> oldValues,
    IDictionary<string, object?> newValues) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ItemId => itemId;
    public long UpdatedBy => updatedBy;
    public IDictionary<string, object?> OldValues => oldValues;
    public IDictionary<string, object?> NewValues => newValues;
}
