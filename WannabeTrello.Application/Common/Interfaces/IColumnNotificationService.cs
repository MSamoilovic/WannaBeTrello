namespace WannabeTrello.Application.Common.Interfaces;

public interface IColumnNotificationService
{
    Task NotifyColumnCreated(long createdColumnId, string? columnName, long boardId, long creatorUserId);
    Task NotifyColumnUpdated(long modifiedColumnId, long modifierUserId);
}