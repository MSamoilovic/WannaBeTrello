using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Board_Events;

namespace Feezbow.Application.Features.Events.Board;

public class BoardRestoredEventHandler(
    IBoardNotificationService boardNotificationService) : INotificationHandler<BoardRestoredEvent>
{
    public async Task Handle(BoardRestoredEvent notification, CancellationToken cancellationToken)
    {
        await boardNotificationService.NotifyBoardRestored(notification.BoardId, notification.ModifierUserId);
    }
}