namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListCreatedEvent(long shoppingListId, long projectId, string name, long createdBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ProjectId => projectId;
    public string Name => name;
    public long CreatedBy => createdBy;
}
