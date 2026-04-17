namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListArchivedEvent(long shoppingListId, long projectId, long archivedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ProjectId => projectId;
    public long ArchivedBy => archivedBy;
}
