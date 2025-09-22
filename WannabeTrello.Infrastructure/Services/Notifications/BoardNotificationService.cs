using Microsoft.AspNetCore.SignalR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.SignalR;

namespace WannabeTrello.Infrastructure.Services.Notifications;

public class BoardNotificationService(IHubContext<TrellyHub, ITrellyHub> hubContext): IBoardNotificationService
{
    public async Task NotifyBoardCreated(long createdBoardId, string? boardName, long createdUserId)
    {
        await hubContext.Clients.All.BoardCreated(createdBoardId, boardName, createdUserId);
    }

    public async Task NotifyBoardUpdated(long createdBoardId, long modifierUserId)
    {
        await hubContext.Clients.All.BoardUpdated(createdBoardId, modifierUserId);
    }

    public async Task NotifyBoardArchived(long archivedBoardId, long modifierUserId)
    {
        await hubContext.Clients.All.BoardArchived(archivedBoardId, modifierUserId);
    }

    public async Task NotifyBoardRestored(long restoredBoardId, long modifierUserId)
    {
        await hubContext.Clients.All.BoardRestored(restoredBoardId, modifierUserId);
    }
}