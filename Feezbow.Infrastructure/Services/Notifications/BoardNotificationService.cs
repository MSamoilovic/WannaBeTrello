using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class BoardNotificationService(
    IHubContext<BoardHub, IBoardHubClient> boardHub,
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<BoardNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IBoardNotificationService
{
    public async Task NotifyBoardCreated(long boardId, long projectId, string? boardName, long creatorUserId)
    {
        await SendAsync(_ => new ValueTask(projectHub.Clients
            .Group($"Project:{projectId}")
            .BoardCreated(new BoardCreatedNotification
            {
                BoardId = boardId,
                BoardName = boardName ?? string.Empty,
                CreatedBy = creatorUserId
            })));
    }

    public async Task NotifyBoardUpdated(long boardId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .BoardUpdated(new BoardUpdatedNotification
            {
                BoardId = boardId,
                ModifiedBy = modifierUserId
            })));
    }

    public async Task NotifyBoardArchived(long boardId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .BoardArchived(new BoardArchivedNotification
            {
                BoardId = boardId,
                ArchivedBy = modifierUserId
            })));
    }

    public async Task NotifyBoardRestored(long boardId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .BoardRestored(new BoardRestoredNotification
            {
                BoardId = boardId,
                RestoredBy = modifierUserId
            })));
    }
}
