using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class ColumnNotificationService(IHubContext<TrellyHub, ITrellyHub> hubContext): IColumnNotificationService
{
    public async Task NotifyColumnCreated(long createdColumnId, string? columnName, long boardId, long creatorUserId)
    {
        await hubContext.Clients.All.ColumnCreated(boardId, createdColumnId, columnName, creatorUserId);
    }

    public async Task NotifyColumnUpdated(long columnId, string oldName, string newName, long boardId, long modifierUserId)
    {
        await hubContext.Clients.All.ColumnUpdated(boardId, columnId, oldName, newName, modifierUserId);
    }

    public async Task NotifyColumnOrderChanged(long columnId, long boardId, int oldOrder, int newOrder, long modifierUserId)
    {
        await hubContext.Clients.All.ColumnOrderChanged(boardId, columnId, oldOrder, newOrder, modifierUserId);
    }

    public async Task NotifyColumnWipLimitChanged(long columnId, long boardId, int? oldWipLimit, int? newWipLimit, long modifierUserId)
    {
        await hubContext.Clients.All.ColumnWipLimitChanged(boardId, columnId, oldWipLimit, newWipLimit, modifierUserId);
    }
}