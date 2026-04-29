namespace Feezbow.Application.Common.Interfaces;

public interface IShoppingListNotificationService
{
    Task NotifyListCreated(long shoppingListId, long projectId, string name, long createdBy, CancellationToken cancellationToken = default);
    Task NotifyListRenamed(long shoppingListId, long projectId, string oldName, string newName, long modifiedBy, CancellationToken cancellationToken = default);
    Task NotifyListArchived(long shoppingListId, long projectId, long archivedBy, CancellationToken cancellationToken = default);
    Task NotifyItemAdded(long shoppingListId, long projectId, long itemId, string name, long addedBy, CancellationToken cancellationToken = default);
    Task NotifyItemUpdated(long shoppingListId, long projectId, long itemId, long modifiedBy, IDictionary<string, object?> changes, CancellationToken cancellationToken = default);
    Task NotifyItemRemoved(long shoppingListId, long projectId, long itemId, long removedBy, CancellationToken cancellationToken = default);
    Task NotifyItemPurchased(long shoppingListId, long projectId, long itemId, long purchasedBy, CancellationToken cancellationToken = default);
    Task NotifyItemUnpurchased(long shoppingListId, long projectId, long itemId, long modifiedBy, CancellationToken cancellationToken = default);
}
