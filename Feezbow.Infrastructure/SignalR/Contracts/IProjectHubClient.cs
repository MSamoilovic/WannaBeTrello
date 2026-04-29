namespace Feezbow.Infrastructure.SignalR.Contracts;

/// <summary>
/// Strongly-typed client contract for project-related real-time notifications.
/// Clients subscribed to project groups receive these events.
/// </summary>
public interface IProjectHubClient
{
    Task ProjectCreated(ProjectCreatedNotification notification);
    Task ProjectUpdated(ProjectUpdatedNotification notification);
    Task ProjectArchived(ProjectArchivedNotification notification);
    Task MemberAdded(ProjectMemberAddedNotification notification);
    Task MemberRemoved(ProjectMemberRemovedNotification notification);
    Task MemberUpdated(ProjectMemberUpdatedNotification notification);
    Task BoardCreated(BoardCreatedNotification notification);

    // Bills
    Task BillCreated(BillCreatedNotification notification);
    Task BillUpdated(BillUpdatedNotification notification);
    Task BillPaid(BillPaidNotification notification);
    Task BillSplitPaid(BillSplitPaidNotification notification);
    Task BillDeleted(BillDeletedNotification notification);

    // Chores
    Task ChoreCreated(ChoreCreatedNotification notification);
    Task ChoreUpdated(ChoreUpdatedNotification notification);
    Task ChoreAssigned(ChoreAssignedNotification notification);
    Task ChoreCompleted(ChoreCompletedNotification notification);
    Task ChoreDeleted(ChoreDeletedNotification notification);

    // Shopping lists
    Task ShoppingListCreated(ShoppingListCreatedNotification notification);
    Task ShoppingListRenamed(ShoppingListRenamedNotification notification);
    Task ShoppingListArchived(ShoppingListArchivedNotification notification);
    Task ShoppingListItemAdded(ShoppingListItemAddedNotification notification);
    Task ShoppingListItemUpdated(ShoppingListItemUpdatedNotification notification);
    Task ShoppingListItemRemoved(ShoppingListItemRemovedNotification notification);
    Task ShoppingListItemPurchased(ShoppingListItemPurchasedNotification notification);
    Task ShoppingListItemUnpurchased(ShoppingListItemUnpurchasedNotification notification);

    // Household
    Task HouseholdProfileCreated(HouseholdProfileCreatedNotification notification);
    Task HouseholdProfileUpdated(HouseholdProfileUpdatedNotification notification);
    Task HouseholdMemberRoleAssigned(HouseholdMemberRoleAssignedNotification notification);
    Task HouseholdMemberRoleRemoved(HouseholdMemberRoleRemovedNotification notification);

    // Attachments
    Task AttachmentUploaded(AttachmentUploadedNotification notification);
    Task AttachmentDeleted(AttachmentDeletedNotification notification);
}
