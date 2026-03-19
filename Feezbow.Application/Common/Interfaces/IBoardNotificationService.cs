namespace Feezbow.Application.Common.Interfaces;

public interface IBoardNotificationService
{
    Task NotifyBoardCreated(long boardId, long projectId, string? boardName, long creatorUserId);
    Task NotifyBoardUpdated(long boardId, long modifierUserId);
    Task NotifyBoardArchived(long boardId, long modifierUserId);
    Task NotifyBoardRestored(long boardId, long modifierUserId);
}