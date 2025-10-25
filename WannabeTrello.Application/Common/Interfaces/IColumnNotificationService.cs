namespace WannabeTrello.Application.Common.Interfaces;

public interface IColumnNotificationService
{
    Task NotifyColumnCreated(long createdColumnId, string? columnName, long boardId, long creatorUserId);
    Task NotifyColumnUpdated(long columnId, string oldName, string newName, long boardId, long modifierUserId);
    Task NotifyColumnOrderChanged(long columnId, long boardId, int oldOrder, int newOrder, long modifierUserId);
    Task NotifyColumnWipLimitChanged(long columnId, long boardId, int? oldWipLimit, int? newWipLimit, long modifierUserId);
}