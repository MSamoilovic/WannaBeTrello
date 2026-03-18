namespace WannabeTrello.Infrastructure.SignalR.Contracts;

/// <summary>
/// Strongly-typed client contract for board-related real-time notifications.
/// Clients subscribed to board groups receive these events.
/// </summary>
public interface IBoardHubClient
{
    
    Task BoardCreated(BoardCreatedNotification notification);
    Task BoardUpdated(BoardUpdatedNotification notification);
    Task BoardArchived(BoardArchivedNotification notification);
    Task BoardRestored(BoardRestoredNotification notification);

    
    Task ColumnCreated(ColumnCreatedNotification notification);
    Task ColumnUpdated(ColumnUpdatedNotification notification);
    Task ColumnOrderChanged(ColumnOrderChangedNotification notification);
    Task ColumnWipLimitChanged(ColumnWipLimitChangedNotification notification);
    Task ColumnDeleted(ColumnDeletedNotification notification);

    
    Task TaskCreated(TaskCreatedNotification notification);
    Task TaskUpdated(TaskUpdatedNotification notification);
    Task TaskMoved(TaskMovedNotification notification);
    Task TaskAssigned(TaskAssignedNotification notification);

    Task CommentAdded(CommentAddedNotification notification);
    Task CommentUpdated(CommentUpdatedNotification notification);
    Task CommentDeleted(CommentDeletedNotification notification);
    Task CommentRestored(CommentRestoredNotification notification);
}
