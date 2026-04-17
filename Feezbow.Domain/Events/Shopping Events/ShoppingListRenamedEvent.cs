namespace Feezbow.Domain.Events.Shopping_Events;

public class ShoppingListRenamedEvent(long shoppingListId, long projectId, string oldName, string newName, long updatedBy) : DomainEvent
{
    public long ShoppingListId => shoppingListId;
    public long ProjectId => projectId;
    public string OldName => oldName;
    public string NewName => newName;
    public long UpdatedBy => updatedBy;
}
