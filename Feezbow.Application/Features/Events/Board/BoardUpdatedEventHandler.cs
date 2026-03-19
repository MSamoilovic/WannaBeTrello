using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events;

namespace Feezbow.Application.Features.Events.Board;

public class BoardUpdatedEventHandler(
    IBoardNotificationService notificationService)
    : INotificationHandler<BoardUpdatedEvent>
{
    public async Task Handle(BoardUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.NotifyBoardUpdated(notification.BoardId, notification.ModifierUserId);
    }
}