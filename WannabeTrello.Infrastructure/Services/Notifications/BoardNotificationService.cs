using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class BoardNotificationService(IHubContext<TrellyHub, ITrellyHub> hubContext): IBoardNotificationService
{
    public async Task NotifyBoardCreated(long createdBoardId, string boardName)
    {
        await hubContext.Clients.All.BoardCreated(createdBoardId, boardName);
    }

    public async Task NotifyBoardUpdated(long createdBoardId, long modifierUserId)
    {
        await hubContext.Clients.All.BoardUpdated(createdBoardId, modifierUserId);
    }
}