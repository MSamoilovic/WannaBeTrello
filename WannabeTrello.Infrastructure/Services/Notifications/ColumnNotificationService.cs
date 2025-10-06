using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class ColumnNotificationService(IHubContext<TrellyHub, ITrellyHub> hubContext): IColumnNotificationService
{
    public async Task NotifyColumnCreated(long createdColumnId, string? columnName, long boardId, long creatorUserId)
    {
       await hubContext.Clients.All.ColumnCreated(boardId,createdColumnId, columnName, creatorUserId);
    }

    public Task NotifyColumnUpdated(long modifiedColumnId, long modifierUserId)
    {
        throw new NotImplementedException();
    }
}