namespace WannabeTrello.Application.Common.Interfaces;

public interface IBoardNotificationService
{
    Task NotifyBoardCreated(long createdBoardId, string? boardName, long creatorUserId);
    Task NotifyBoardUpdated(long createdBoardId, long modifierUserId);
    Task NotifyBoardArchived(long archivedBoardId, long modifierUserId);
}