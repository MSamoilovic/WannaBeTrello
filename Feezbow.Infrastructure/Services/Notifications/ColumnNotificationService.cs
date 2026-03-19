using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class ColumnNotificationService(
    IHubContext<BoardHub, IBoardHubClient> boardHub,
    ResiliencePipeline pipeline,
    ILogger<ColumnNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IColumnNotificationService
{
    public async Task NotifyColumnCreated(long createdColumnId, string? columnName, long boardId, long creatorUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .ColumnCreated(new ColumnCreatedNotification
            {
                BoardId = boardId,
                ColumnId = createdColumnId,
                ColumnName = columnName ?? string.Empty,
                CreatedBy = creatorUserId
            })));
    }

    public async Task NotifyColumnUpdated(long columnId, string oldName, string newName, long boardId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .ColumnUpdated(new ColumnUpdatedNotification
            {
                BoardId = boardId,
                ColumnId = columnId,
                OldName = oldName,
                NewName = newName,
                ModifiedBy = modifierUserId
            })));
    }

    public async Task NotifyColumnOrderChanged(long columnId, long boardId, int oldOrder, int newOrder, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .ColumnOrderChanged(new ColumnOrderChangedNotification
            {
                BoardId = boardId,
                ColumnId = columnId,
                OldOrder = oldOrder,
                NewOrder = newOrder,
                ModifiedBy = modifierUserId
            })));
    }

    public async Task NotifyColumnWipLimitChanged(long columnId, long boardId, int? oldWipLimit, int? newWipLimit, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .ColumnWipLimitChanged(new ColumnWipLimitChangedNotification
            {
                BoardId = boardId,
                ColumnId = columnId,
                OldWipLimit = oldWipLimit,
                NewWipLimit = newWipLimit,
                ModifiedBy = modifierUserId
            })));
    }

    public async Task NotifyColumnDeletedEvent(long columnId, long boardId, long modifierUserId)
    {
        await SendAsync(_ => new ValueTask(boardHub.Clients
            .Group($"Board:{boardId}")
            .ColumnDeleted(new ColumnDeletedNotification
            {
                BoardId = boardId,
                ColumnId = columnId,
                DeletedBy = modifierUserId
            })));
    }
}
